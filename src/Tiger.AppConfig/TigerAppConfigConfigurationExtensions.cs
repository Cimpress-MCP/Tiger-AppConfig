// <copyright file="TigerAppConfigConfigurationExtensions.cs" company="Cimpress, Inc.">
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

// note(cosborn) Hidden in plain sight to avoid casual misuse of this extension method.
namespace Tiger.AppConfig.Lambda;

/// <summary>Extensions to the functionality of the <see cref="IConfiguration"/> interface.</summary>
public static class TigerAppConfigConfigurationExtensions
{
    /// <summary>
    /// Blocks execution of a Lambda Function until any instances of <see cref="AppConfigConfigurationProvider"/>
    /// added to the configuration have completed a reload of configuration from the AppConfig extensions if such
    /// a reload is in progress when this method is invoked.
    /// </summary>
    /// <param name="configuration">The configuration containing the providers for which to wait.</param>
    /// <param name="cancellationToken">A token to watch for operation cancellation.</param>
    /// <returns>A task which, when resolved, represents operation completion.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static async Task WaitForAppConfigReloadToCompleteAsync(
        this IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (configuration is not IConfigurationRoot configurationRoot)
        {
            return;
        }

        var tasks = configurationRoot
            .Providers
            .OfType<AppConfigConfigurationProvider>()
            .Select(p => p.WaitForReloadToCompleteAsync(cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
