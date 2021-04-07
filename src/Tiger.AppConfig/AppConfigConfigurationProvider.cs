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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Tiger.AppConfig;
using static System.Globalization.CultureInfo;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>Provides AWS Secrets Manager configuration key/values for an application.</summary>
    public sealed class AppConfigConfigurationProvider
        : ConfigurationProvider, IDisposable
    {
        readonly HttpClient _httpClient;
        readonly AppConfigOptions _appConfigOpts;

        /// <summary>Initializes a new instance of the <see cref="AppConfigConfigurationProvider"/> class.</summary>
        /// <param name="configurationSource">The source of AWS Secrets Manager configuration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configurationSource"/> is <see langword="null"/>.</exception>
        public AppConfigConfigurationProvider(AppConfigConfigurationSource configurationSource)
        {
            if (configurationSource is not { } cs)
            {
                throw new ArgumentNullException(nameof(configurationSource));
            }

            _httpClient = cs.HttpClient;
            _appConfigOpts = cs.Options;
        }

        /// <inheritdoc/>
        // because(cosborn) Configuration is purely a sync API, and we want good exceptions. Gross, gross, gross.
        public override void Load() => Task.Run(LoadCoreAsync).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public void Dispose() => _httpClient.Dispose();

        static SortedDictionary<string, string?> NormalizeData(JsonElement root)
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

            var data = new SortedDictionary<string, string?>();
            VisitObject(root, ImmutableArray<string>.Empty, data);
            return data;

            static void VisitObject(JsonElement @object, ImmutableArray<string> context, IDictionary<string, string?> data)
            {
                foreach (var property in @object.EnumerateObject())
                {
                    VisitValue(property.Value, context.Add(property.Name), data);
                }
            }

            static void VisitArray(JsonElement array, ImmutableArray<string> context, IDictionary<string, string?> data)
            {
                foreach (var (elem, idx) in array.EnumerateArray().Select((e, i) => (e, i)))
                {
                    /* note(cosborn)
                     * Remember, configuration considers arrays to be objects with "numeric" indices.
                     * That's why they merge how they do in AppSettings.
                     */
                    VisitValue(elem, context.Add(idx.ToString(InvariantCulture)), data);
                }
            }

            static void VisitValue(JsonElement? value, ImmutableArray<string> context, IDictionary<string, string?> data)
            {
                switch (value)
                {
                    case { ValueKind: JsonValueKind.Object } v:
                        VisitObject(v, context, data);
                        break;
                    case { ValueKind: JsonValueKind.Array } v:
                        VisitArray(v, context, data);
                        break;
                    case { ValueKind: JsonValueKind.Number or JsonValueKind.String or JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null }:
                    case null:
                        var key = ConfigurationPath.Combine(context);

                        // note(cosborn) If you create JSON with duplicate keys, you get what you get.
                        data[key] = value?.ToString();
                        break;
                    case { ValueKind: var vk }:
                        throw new FormatException($"Unsupported JSON token '{vk}' was found.");
                }
            }
        }

        async Task LoadCoreAsync()
        {
            var relativeUri = $"/applications/{_appConfigOpts.Application}/environments/{_appConfigOpts.Environment}/configurations/{_appConfigOpts.Configuration}";
            var uriBuilder = new UriBuilder(Uri.UriSchemeHttp, "localhost", _appConfigOpts.HttpPort, relativeUri);
            using var doc = await _httpClient.GetFromJsonAsync<JsonDocument>(uriBuilder.Uri).ConfigureAwait(false);
            if (doc is not { RootElement: { ValueKind: JsonValueKind.Object } root })
            {
                throw new FormatException($"Top-level JSON element must be an object. Instead, '{doc?.RootElement.ValueKind}' was found.");
            }

            Data = NormalizeData(root);
            OnReload();
        }
    }
}
