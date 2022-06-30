// <copyright file="ConfigurationKey.cs" company="Cimpress, Inc.">
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

/// <summary>Represents a single-level configuration key.</summary>
public readonly struct ConfigurationKey
    : IEquatable<ConfigurationKey>
{
    /// <summary>Initializes a new instance of the <see cref="ConfigurationKey"/> struct.</summary>
    /// <param name="configurationKey">The raw configuration key.</param>
    public ConfigurationKey(string configurationKey)
    {
        Get = configurationKey ?? throw new ArgumentNullException(nameof(configurationKey));
    }

    /// <summary>Gets the underlying value.</summary>
    public string Get { get; }

    /// <summary>Converts a configuation key to a string.</summary>
    /// <param name="ck">The configuration key to convert.</param>
    public static implicit operator string(in ConfigurationKey ck) => ck.Get;

    /// <summary>Tests two instances of the <see cref="ConfigurationKey"/> struct for equality.</summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(ConfigurationKey left, ConfigurationKey right) => left.Equals(right);

    /// <summary>Tests two instances of the <see cref="ConfigurationKey"/> struct for inequality.</summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(ConfigurationKey left, ConfigurationKey right) => !(left == right);

    /// <inheritdoc/>
    public bool Equals(ConfigurationKey other) => Get == other.Get;

    /// <inheritdoc/>
    public override string ToString() => Get;

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is ConfigurationKey configurationKey && Equals(configurationKey);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Get);
}
