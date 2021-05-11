// <copyright file="AppConfigConfigurationProvider.cs" company="Cimpress, Inc.">
//   Copyright 2021 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
#if NET5_0
using System.Net.Http.Json;
#endif
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Nito.AsyncEx;
using Tiger.AppConfig;
using static System.Globalization.CultureInfo;
using static System.Uri;
using static Tiger.AppConfig.Resources;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>Provides AWS AppConfig configuration key/values for an application.</summary>
    public sealed class AppConfigConfigurationProvider
        : ConfigurationProvider, IDisposable
    {
        readonly HttpClient _httpClient;
        readonly AppConfigOptions _appConfigOpts;
        readonly ConfigurationEqualityComparer _equalityComparer;
        readonly IDisposable _subscription;

        readonly AsyncManualResetEvent _reloadEvent = new(set: true);

        int _dataHashCode;

        /// <summary>Initializes a new instance of the <see cref="AppConfigConfigurationProvider"/> class.</summary>
        /// <param name="configurationSource">The source of AWS AppConfig configuration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configurationSource"/> is <see langword="null"/>.</exception>
        public AppConfigConfigurationProvider(AppConfigConfigurationSource configurationSource)
        {
            if (configurationSource is not { } cs)
            {
                throw new ArgumentNullException(nameof(configurationSource));
            }

            _httpClient = cs.HttpClient;
            _appConfigOpts = cs.Options;
            _equalityComparer = cs.EqualityComparer;

            /* note(cosborn)
             * I originally had an implementation here which used a
             * System.Threading.Timer to avoid potentially leaking the
             * cancellation token source. But it may be that this
             * caused invocations to pile up on each other.
             *
             * I don't know whether we're OK on leaking the CTS; Microsoft
             * oddly only calls out using `CancelAfter`, not the constructor
             * which accepts a `TimeSpan`. But... I can't imagine their
             * working any differently from one another, so I'm not sure.
             */

            _subscription = ChangeToken.OnChange(
                () => new CancellationChangeToken(new CancellationTokenSource(_appConfigOpts.PollInterval).Token),
                Reload,
                this);
        }

        /// <summary>
        /// Blocks the configuration provider until reload of configuration is complete
        /// if a reload has already begun.
        /// </summary>
        /// <remarks><para>
        /// This method is not for general use; it is exposed so that a Lambda Function can wait
        /// for the reload to complete before completing the event causing the Lambda compute
        /// environment to be frozen.
        /// </para></remarks>
        /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
        /// <returns>A task which, when resolved, represents operation completion.</returns>
        public Task WaitForReloadToCompleteAsync(CancellationToken cancellationToken = default) =>
            _reloadEvent.WaitAsync(cancellationToken);

        /// <inheritdoc/>
        public override void Load() => // because(cosborn) Configuration is sync – we want good exceptions.
            Task.Run(LoadCoreAsync).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override void Set(string key, string value)
        {
            base.Set(key, value);
            _dataHashCode = _equalityComparer.GetHashCode(Data);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _httpClient.Dispose();
            _subscription.Dispose();
        }

        // hack(cosborn) async void is una-void-able because it's an event handler.
        static async void Reload(AppConfigConfigurationProvider provider)
        {
            provider._reloadEvent.Reset();
            try
            {
                await provider.LoadCoreAsync().ConfigureAwait(false);
            }
            finally
            {
                provider._reloadEvent.Set();
            }
        }

        async Task LoadCoreAsync()
        {
            var uriBuilder = new UriBuilder(
                scheme: UriSchemeHttp,
                host: "localhost",
                port: _appConfigOpts.HttpPort,
                pathValue: _appConfigOpts.Path);
            try
            {
#if NET5_0
                using var doc = await _httpClient.GetFromJsonAsync<JsonDocument>(uriBuilder.Uri).ConfigureAwait(false);
#else
                // note(cosborn) The netcoreapp3.1 version of GetFromJsonAsync bails on non-JSON content-types.
                // todo(cosborn) Why does the extension send application/octet-stream? It knows the content type.
                var rawDoc = await _httpClient.GetStringAsync(uriBuilder.Uri).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(rawDoc);
#endif
                if (doc is not { RootElement: { ValueKind: JsonValueKind.Object } root })
                {
                    throw new FormatException(string.Format(InvariantCulture, NotObject, doc?.RootElement.ValueKind));
                }

                // note(cosborn) JsonDocuments lack well-behaved equality, so we have to normalize first.
                var updatedData = NormalizeData(root);
                var updatedDataHashCode = _equalityComparer.GetHashCode(updatedData);

                if (updatedDataHashCode != _dataHashCode || !_equalityComparer.Equals(updatedData, Data))
                {
                    Data = updatedData;
                    _dataHashCode = updatedDataHashCode;
                    OnReload();
                }
            }
#if NET5_0
            catch (HttpRequestException hre)
            {
                Log(string.Format(InvariantCulture, FailedToContact, hre.StatusCode));
            }
            catch (OperationCanceledException oce) when (oce.InnerException is TimeoutException)
            {
                Log(TimedOut);
            }
#else
            catch (HttpRequestException)
            {
                LogAndContinue(FailedToContact);
            }
#endif
            catch (OperationCanceledException)
            {
                /* note(cosborn)
                 * In netcoreapp3.1 or less, this *could* mean a timeout.
                 * No way of knowing, unfortunately.
                 */
                Log(Canceled);
            }
            catch (JsonException)
            {
                Log(DeserializationFailed);
                throw;
            }

            static void Log(string message)
            {
                Console.WriteLine(Preamble, message);
                Console.WriteLine(UsingCache);
            }
        }

        IDictionary<string, string> NormalizeData(JsonElement root)
        {
            /* note(cosborn)
             * "Normalize" the data? But it's already in proper JSON form. Why?
             *
             * The configuration providers that read JSON from files allow
             * configuration to be specified in two ways. Normalization means that:
             *
             * { "Compound": { "Key": "value" } }
             *
             * ...and:
             *
             * { "Compound:Key": "value" }
             *
             * ...are equivalent, and the latter is canonical. I do the same
             * operations here, as it's expected. (It's still read back as
             * a JSON object, natch.) If raw JSON is entered directly, however,
             * we'll still do the right thing.
             */

            var data = new Dictionary<string, string>(_equalityComparer.KeyComparer);
            VisitObject(root, ImmutableArray<string>.Empty, data);
            return data;

            static void VisitObject(JsonElement @object, ImmutableArray<string> context, Dictionary<string, string> data)
            {
                foreach (var property in @object.EnumerateObject())
                {
                    VisitValue(property.Value, context.Add(property.Name), data);
                }
            }

            static void VisitArray(JsonElement array, ImmutableArray<string> context, Dictionary<string, string> data)
            {
                for (var i = 0; i < array.GetArrayLength(); i++)
                {
                    /* note(cosborn)
                     * Remember, configuration considers arrays to be objects with "numeric" indices.
                     * That's why they merge how they do in AppSettings.
                     */
                    VisitValue(array[i], context.Add(i.ToString(InvariantCulture)), data);
                }
            }

            static void VisitValue(JsonElement value, ImmutableArray<string> context, Dictionary<string, string> data)
            {
                switch (value)
                {
                    case { ValueKind: JsonValueKind.Object } v:
                        VisitObject(v, context, data);
                        break;
                    case { ValueKind: JsonValueKind.Array } v:
                        VisitArray(v, context, data);
                        break;
                    case { ValueKind: JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null } v:
                        var key = ConfigurationPath.Combine(context);

                        // note(cosborn) If you create JSON with duplicate keys, you get what you get.
                        data[key] = v.ToString() ?? string.Empty;
                        break;
                    case { ValueKind: var vk }:
                        throw new FormatException($"Unsupported JSON token '{vk}' was found.");
                }
            }
        }
    }
}
