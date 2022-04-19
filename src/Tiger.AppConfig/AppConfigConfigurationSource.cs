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

namespace Microsoft.Extensions.Configuration;

/// <summary>A source of AWS AppConfig configuration key/values for an application.</summary>
/// <param name="HttpClient">A client for communicating with the AWS AppConfig extension.</param>
/// <param name="Options">The application's options for AWS AppConfig configuration.</param>
public sealed record class AppConfigConfigurationSource(HttpClient HttpClient, AppConfigOptions Options)
    : IConfigurationSource
{
    /// <summary>
    /// Gets the equality comparer used to determine whether configuration should be reloaded.
    /// </summary>
    public ConfigurationEqualityComparer EqualityComparer { get; } = new();

    /// <inheritdoc/>
    IConfigurationProvider IConfigurationSource.Build(IConfigurationBuilder builder) =>
        new AppConfigConfigurationProvider(this);
}
