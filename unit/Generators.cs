// <copyright file="Generators.cs" company="Cimpress, Inc.">
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

using FsCheck.Fluent;
using Tiger.AppConfig;
using static System.StringComparison;
using static FsCheck.Fluent.ArbMap;
using static Microsoft.Extensions.Configuration.ConfigurationPath;

namespace Test;

static class Generators
{
    public static Arbitrary<ConfigurationKey> ConfigurationKey { get; } = Default.ArbFor<NonEmptyString>()
        .Filter(nes => !nes.Get.Contains(KeyDelimiter, Ordinal))
        .Convert(nes => new ConfigurationKey(nes.Get), ck => NonEmptyString.NewNonEmptyString(ck.ToString()));

    public static Arbitrary<AppConfigOptions> AppConfigOptions { get; } = Arb.From(
        from paths in Default.GeneratorFor<NonEmptyString>().Three()
        from port in Gen.Choose(80, 5000)
        select new AppConfigOptions
        {
            Application = paths.Item1.Get,
            Environment = paths.Item2.Get,
            ConfigurationProfile = paths.Item3.Get,
            HttpPort = port,
        });
}
