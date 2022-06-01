// <copyright file="SpanExtensions.cs" company="Cimpress, Inc.">
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

public static class SpanExtensions
{
    public static void Deconstruct<TElement>(this ReadOnlySpan<TElement> readOnlySpan, out TElement head, out ReadOnlySpan<TElement> tail)
    {
        if (readOnlySpan.Length == 0)
        {
            throw new ArgumentException("Empty collection cannot be deconstructed.", nameof(readOnlySpan));
        }

        head = readOnlySpan[0];
        tail = readOnlySpan[1..];
    }
}
