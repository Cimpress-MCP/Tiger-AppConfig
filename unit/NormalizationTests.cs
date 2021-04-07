// <copyright file="NormalizationTests.cs" company="Cimpress, Inc.">
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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Tiger.AppConfig;
using Xunit;
using static System.Net.HttpStatusCode;
using static System.Net.Mime.MediaTypeNames;

namespace Test
{
    /// <summary>Tests of configuration data normalization.</summary>
    [Properties(Arbitrary = new[] { typeof(Generators) }, QuietOnSuccess = true)]
    public static class NormalizationTests
    {
        static readonly Encoding s_utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        [Property(DisplayName = "A plain value is unchanged by normalization.")]
        public static void PlainValue_Unchanged(ConfigurationKey key, NonEmptyString value, NonNull<AppConfigOptions> appConfigOpts)
        {
            var datum = new Dictionary<string, string>
            {
                [key.Get] = value.Get,
            };
            var handler = Parrot(datum);
            using var httpClient = new HttpClient(handler.Object);

            var configurationSource = new AppConfigConfigurationSource(httpClient, appConfigOpts.Get);
            using var sut = new AppConfigConfigurationProvider(configurationSource);
            sut.Load();

            handler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
            Assert.True(sut.TryGet(key.Get, out var actual));
            Assert.Equal(value.Get, actual);
        }

        [Property(DisplayName = "A compound value is unchanged by normalization.")]
        public static void CompoundValue_Normalized(ConfigurationKey[] key, NonEmptyString value, NonNull<AppConfigOptions> appConfigOpts)
        {
            var compoundKey = ConfigurationPath.Combine(key.Select(k => k.Get));
            var datum = new Dictionary<string, string>
            {
                [compoundKey] = value.Get,
            };
            var handler = Parrot(datum);
            using var httpClient = new HttpClient(handler.Object);

            var configurationSource = new AppConfigConfigurationSource(httpClient, appConfigOpts.Get);
            using var sut = new AppConfigConfigurationProvider(configurationSource);
            sut.Load();

            handler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
            Assert.True(sut.TryGet(compoundKey, out var actual));
            Assert.Equal(value.Get, actual);
        }

        [Property(DisplayName = "A deep value is normalized.")]
        public static void DeepValue_Normalized(
            NonEmptyArray<ConfigurationKey> key,
            NonEmptyString value,
            NonNull<AppConfigOptions> appConfigOpts)
        {
            var datum = GenerateDatum(key.Get, value.Get);
            var handler = Parrot(datum);
            using var httpClient = new HttpClient(handler.Object);

            var configurationSource = new AppConfigConfigurationSource(httpClient, appConfigOpts.Get);
            using var sut = new AppConfigConfigurationProvider(configurationSource);
            sut.Load();

            var compoundKey = ConfigurationPath.Combine(key.Get.Select(k => k.Get));
            handler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
            Assert.True(sut.TryGet(compoundKey, out var actual));
            Assert.Equal(value.Get, actual);

            static ImmutableDictionary<string, object> GenerateDatum(ReadOnlySpan<ConfigurationKey> k, string v)
            {
                var (head, tail) = k;

                var pair = tail.Length == 0
                    ? KeyValuePair.Create<string, object>(head.Get, v)
                    : KeyValuePair.Create<string, object>(head.Get, GenerateDatum(tail, v));

                return ImmutableDictionary.CreateRange(new[] { pair });
            }
        }

        static Mock<HttpMessageHandler> Parrot<T>(T body)
        {
            var handler = new Mock<HttpMessageHandler>();
            _ = handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => new HttpResponseMessage(OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(body), s_utf8, Application.Octet),
                });
            return handler;
        }
    }
}
