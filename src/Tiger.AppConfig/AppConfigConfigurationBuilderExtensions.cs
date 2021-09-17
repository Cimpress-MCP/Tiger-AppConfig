// <copyright file="AppConfigConfigurationBuilderExtensions.cs" company="Cimpress, Inc.">
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
using System.Net.Http;
using Tiger.AppConfig;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>Extends the functionality of <see cref="IConfigurationBuilder"/> for AWS AppConfig.</summary>
    public static class AppConfigConfigurationBuilderExtensions
    {
        /* note(cosborn)
         * Lacking dependency injection, we will manage the lifetime of the
         * inner HTTP handler ourselves.
         *
         * Good news, everyone! We only ever contact "localhost", so it can
         * sit without modification or replacement, and the pooled connection
         * can retain its default infinite lifetime (`PooledConnectionLifetime`).
         */
        static readonly HttpMessageHandler s_handler = new SocketsHttpHandler();

        /// <summary>Adds AWS AppConfig as a configuration source.</summary>
        /// <param name="builder">The configuration builder to which to add.</param>
        /// <param name="configurationSection">
        /// The name of the configuration section at which to find configuration.
        /// </param>
        /// <returns>The modified configuration builder.</returns>
        public static IConfigurationBuilder AddAppConfig(
            this IConfigurationBuilder builder,
            string configurationSection = AppConfigOptions.AppConfig)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var httpClient = new HttpClient(s_handler, disposeHandler: false);
            var appConfigOpts = GetOptions(builder, configurationSection);
            return builder.Add(new AppConfigConfigurationSource(httpClient, appConfigOpts));
        }

        static AppConfigOptions GetOptions(IConfigurationBuilder builder, string configurationSection)
        {
            const string AppConfigConfigurationKey = "TIGER_CONFIGBUILDER_APPCONFIG";

            if (!builder.Properties.TryGetValue(AppConfigConfigurationKey, out var value)
                || value is not AppConfigOptions appConfigOpts)
            {
                /* note(cosborn)
                 * Building the configuration provider is (relatively) expensive, so
                 * we want to avoid doing it more than is necessary. That sort of in-
                 * band-but-untyped communication is exactly what `Properties` is for.
                 */
                var configuration = builder.AddEnvironmentVariables("AWS_APPCONFIG_EXTENSION_").Build();
                appConfigOpts = configuration.Get<AppConfigOptions>();
                configuration.GetSection(configurationSection).Bind(appConfigOpts);
                builder.Properties.Add(AppConfigConfigurationKey, appConfigOpts);
            }

            return appConfigOpts;
        }
    }
}
