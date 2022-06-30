// <copyright file="TigerAppConfigApplicationBuilderExtensions.cs" company="Cimpress, Inc.">
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

namespace Tiger.AppConfig.AspNetCore;

/// <summary>Extensions to the functionality of the <see cref="IApplicationBuilder"/> interface.</summary>
public static class TigerAppConfigApplicationBuilderExtensions
{
    /// <summary>Causes requests to wait for the AppConfig extension to finish an in-progress reload, if one exists.</summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The modified application builder.</returns>
    public static IApplicationBuilder UseAppConfigReload(this IApplicationBuilder app) =>
        app.UseMiddleware<AppConfigReload>();

    [SuppressMessage("Microsoft.Style", "CA1812", Justification = "Resolved by type activation.")]
    sealed class AppConfigReload
    {
        readonly RequestDelegate _next;

        public AppConfigReload(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IConfiguration configuration)
        {
            await _next(httpContext);
            await configuration.WaitForAppConfigReloadToCompleteAsync(httpContext.RequestAborted);
        }
    }
}
