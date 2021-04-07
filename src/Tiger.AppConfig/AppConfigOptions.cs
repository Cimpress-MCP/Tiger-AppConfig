// <copyright file="AppConfigOptions.cs" company="Cimpress, Inc.">
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

using System.Diagnostics.CodeAnalysis;

namespace Tiger.AppConfig
{
    /// <summary>
    /// Represents the declarative configuration options for AWS AppConfig configuration.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Used with configuration.")]
    [SuppressMessage("Roslynator.Performance", "RCS1170", Justification = "Used with configuration.")]
    public sealed class AppConfigOptions
    {
        /// <summary>The default name of the configuration section.</summary>
        public const string AppConfig = nameof(AppConfig);

        /// <summary>Gets or sets the name of the AppConfig application.</summary>
        public string Application { get; set; } = null!;

        /// <summary>Gets or sets the name of the AppConfig environment.</summary>
        public string Environment { get; set; } = null!;

        /// <summary>Gets or sets the name of the AppConfig profile.</summary>
        public string Configuration { get; set; } = null!;

        /// <summary>Gets or sets the port on which the AppConfig extension is listening.</summary>
        public int HttpPort { get; set; } = 2772;
    }
}
