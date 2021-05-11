// <copyright file="ConfigurationEqualityComparer.cs" company="Cimpress, Inc.">
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

namespace Tiger.AppConfig
{
    /// <summary>Compares configurations for diffing purposes.</summary>
    public sealed class ConfigurationEqualityComparer
        : EqualityComparer<IDictionary<string, string>>
    {
        /* note(cosborn)
         * As in the configuration provider, keys are compared case-insensitively.
         * Values can be different based on casing, however, so we get stricter.
         */

        static readonly IEqualityComparer<string> s_valueComparer = StringComparer.Ordinal;

        /// <summary>
        /// Gets the equality comparer that is used to determine equality of configuration keys.
        /// </summary>
        public IEqualityComparer<string> KeyComparer { get; } = StringComparer.OrdinalIgnoreCase;

        /// <inheritdoc/>
        public override bool Equals(IDictionary<string, string>? x, IDictionary<string, string>? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.Count != y.Count)
            {
                return false;
            }

            foreach (var (key, value) in x)
            {
                if (!y.TryGetValue(key, out var otherValue) || !s_valueComparer.Equals(value, otherValue))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode(IDictionary<string, string> obj)
        {
            if (obj is null)
            {
                return 0;
            }

            var hashCode = default(HashCode);
            foreach (var (key, value) in obj)
            {
                hashCode.Add(key, KeyComparer);
                if (value is { } v)
                {
                    hashCode.Add(v, s_valueComparer);
                }
            }

            return hashCode.ToHashCode();
        }
    }
}
