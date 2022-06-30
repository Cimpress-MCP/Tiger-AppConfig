// <copyright file="AppConfigOptions.cs" company="Cimpress, Inc.">
//   Copyright 2022 Cimpress, Inc.
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

using static System.Threading.Timeout;

namespace Tiger.AppConfig;

/// <summary>
/// Represents the declarative configuration options for AWS AppConfig configuration.
/// </summary>
public sealed class AppConfigOptions
{
    /// <summary>The default name of the configuration section.</summary>
    public const string AppConfig = nameof(AppConfig);

    /// <summary>Gets or sets the name of the AppConfig application.</summary>
    public string Application { get; set; } = null!;

    /// <summary>Gets or sets the name of the AppConfig environment.</summary>
    public string Environment { get; set; } = null!;

    /// <summary>Gets or sets the name of the AppConfig profile.</summary>
    public string ConfigurationProfile { get; set; } = null!;

    /// <summary>Gets or sets the port on which the AppConfig extension is listening.</summary>
    public int HttpPort { get; set; } = 2772;

    /// <summary>
    /// Gets or sets the time after which AppConfig will refresh configuration,
    /// which is also the time after which the client will check for such
    /// refreshed configuration.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 45;

    /// <summary>
    /// Gets the time after which AppConfig will refresh configuration,
    /// which is also the time after which the client will check for such
    /// refreshed configuration.
    /// </summary>
    public TimeSpan PollInterval => PollIntervalSeconds < 0 ? InfiniteTimeSpan : TimeSpan.FromSeconds(PollIntervalSeconds);

    /// <summary>Gets the path from which to retrieve configuration.</summary>
    public string Path => $"/applications/{Application}/environments/{Environment}/configurations/{ConfigurationProfile}";
}
