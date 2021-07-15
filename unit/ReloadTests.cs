// <copyright file="ReloadTests.cs" company="Cimpress, Inc.">
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

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Tiger.AppConfig;
using Xunit;

namespace Test
{
    /// <summary>Tests of reloading configuration.</summary>
    public static class ReloadTests
    {
        [Fact(DisplayName = "If no reload is occurring, the wait should be effectively immediate.")]
        public static async Task NoReload_OK()
        {
            using var httpClient = new HttpClient();
            var configurationSource = new AppConfigConfigurationSource(httpClient, new AppConfigOptions());
            using var sut = new AppConfigConfigurationProvider(configurationSource);

            // note(cosborn) Assertion controlled by the "longRunningTestSeconds" parameter in `xunit.runner.json`.
            await sut.WaitForReloadToCompleteAsync();
        }
    }
}
