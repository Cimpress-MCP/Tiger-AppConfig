// <copyright file="ConfigurationEqualityTests.cs" company="Cimpress, Inc.">
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

namespace Test;

/// <summary>Tests of configuration data equality.</summary>
[Properties(QuietOnSuccess = true)]
public sealed class ConfigurationEqualityTests
{
    readonly ConfigurationEqualityComparer _sut = new();

    [Fact(DisplayName = "Null is equal to null.")]
    public void Null_Equals_Null() => Assert.True(_sut.Equals(null, null));

    [Property(DisplayName = "Identical configurations are equal.")]
    public void Identical_Equals_Equal(Func<int, KeyValuePair<string, string>> pairMap, NonNegativeInt size)
    {
        var config = CreateConfiguration(pairMap, size.Get);
        Assert.Equal(config, config, comparer: _sut);
    }

    [Property(DisplayName = "Equal configurations are equal.")]
    public void Equal_Equals_Equal(Func<int, KeyValuePair<string, string>> pairMap, NonNegativeInt size)
    {
        var config = CreateConfiguration(pairMap, size.Get);
        var copy = new Dictionary<string, string>(config, _sut.KeyComparer);
        Assert.Equal(config, copy, comparer: _sut);
    }

    [Property(DisplayName = "Configurations which differ by count are not equal.")]
    public void DifferentCount_Equals_Unequal(Func<int, KeyValuePair<string, string>> pairMap, NonNegativeInt size, NonEmptyString value)
    {
        var config = CreateConfiguration(pairMap, size.Get);
        var copy = new Dictionary<string, string>(config, _sut.KeyComparer)
        {
            [value.Get] = value.Get,
        };
        Assert.NotEqual(config, copy, comparer: _sut);
    }

    [Property(DisplayName = "Configurations which differ by value are not equal.")]
    public void DifferentValue_Equals_Unequal(Func<int, KeyValuePair<string, string>> pairMap, PositiveInt size, NonEmptyString value)
    {
        var config = CreateConfiguration(pairMap, size.Get);
        var copy = new Dictionary<string, string>(config, _sut.KeyComparer);
        if (config.Count == 0)
        {
            return;
        }

        var first = copy.First();
        copy[first.Key] = first.Value + value.Get;
        Assert.NotEqual(config, copy, comparer: _sut);
    }

    [Fact(DisplayName = "Null has hashcode zero.")]
    public void Null_Hashcode_Zero() => Assert.Equal(0, _sut.GetHashCode(null));

    [Property(DisplayName = "Identical configurations have equal hashcodes.")]
    public void Identical_Hashcode_Equal(Func<int, KeyValuePair<string, string>> pairMap, NonNegativeInt size)
    {
        var config = CreateConfiguration(pairMap, size.Get);
        var before = _sut.GetHashCode(config);
        var after = _sut.GetHashCode(config);
        Assert.Equal(before, after);
    }

    [Property(DisplayName = "Equal configurations have equal hashcodes.")]
    public void Equal_Hashcode_Equal(Func<int, KeyValuePair<string, string>> pairMap, NonNegativeInt size)
    {
        var config = CreateConfiguration(pairMap, size.Get);
        var before = _sut.GetHashCode(config);
        var copy = new Dictionary<string, string>(config, _sut.KeyComparer);
        var after = _sut.GetHashCode(copy);
        Assert.Equal(before, after);
    }

    [Property(DisplayName = "Configurations which differ by count have unequal hashcodes.")]
    public void DifferentCount_Hashcode_Unequal(Func<int, KeyValuePair<string, string>> pairMap, NonNegativeInt size, NonEmptyString value)
    {
        var config = CreateConfiguration(pairMap, size.Get);
        var before = _sut.GetHashCode(config);
        var copy = new Dictionary<string, string>(config, _sut.KeyComparer)
        {
            [value.Get] = value.Get,
        };
        var after = _sut.GetHashCode(copy);
        Assert.NotEqual(before, after);
    }

    [Property(DisplayName = "Configurations which differ by value have unequal hashcodes.")]
    public void DifferentValue_Hashcode_Unequal(Func<int, KeyValuePair<string, string>> pairMap, PositiveInt size, NonEmptyString value)
    {
        var config = CreateConfiguration(pairMap, size.Get);
        var before = _sut.GetHashCode(config);
        var copy = new Dictionary<string, string>(config, _sut.KeyComparer);
        if (config.Count == 0)
        {
            return;
        }

        var first = copy.First();
        copy[first.Key] = first.Value + value.Get;
        var after = _sut.GetHashCode(copy);
        Assert.NotEqual(before, after);
    }

    IDictionary<string, string> CreateConfiguration(
        Func<int, KeyValuePair<string, string>> pairMap,
        int size) => EnumerableEx
            .Generate(0, i => i <= size, i => i + 1, pairMap)
            .GroupBy(kvp => kvp.Key, _sut.KeyComparer)
            .Where(g => g.Key is not null)
            .Select(g => g.First())
            .ToImmutableDictionary(_sut.KeyComparer);
}
