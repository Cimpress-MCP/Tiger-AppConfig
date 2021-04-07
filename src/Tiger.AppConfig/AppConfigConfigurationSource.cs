// <copyright file="AppConfigConfigurationSource.cs" company="Cimpress, Inc.">
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
    /// <summary>A source of AWS AppConfig configuration key/values for an application.</summary>
    public sealed class AppConfigConfigurationSource
        : IConfigurationSource
    {
        /// <summary>Initializes a new instance of the <see cref="AppConfigConfigurationSource"/> class.</summary>
        /// <param name="httpClient">A client for communicating with the AWS AppConfig extension.</param>
        /// <param name="options">The application's configuration options for AWS AppConfig.</param>
        public AppConfigConfigurationSource(
            HttpClient httpClient,
            AppConfigOptions options)
        {
            HttpClient = httpClient;
            Options = options;
        }

        /// <summary>Gets a client for communicating with the AWS AppConfig extension.</summary>
        public HttpClient HttpClient { get; }

        /// <summary>Gets the application's configuration options for AWS AppConfig.</summary>
        public AppConfigOptions Options { get; }

        /// <inheritdoc/>
        IConfigurationProvider IConfigurationSource.Build(IConfigurationBuilder builder) =>
            new AppConfigConfigurationProvider(this);
    }
}
