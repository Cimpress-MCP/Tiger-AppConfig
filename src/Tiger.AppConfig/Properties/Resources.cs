// <copyright file="Resources.cs" company="Cimpress, Inc.">
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

using System.ComponentModel;
using System.Resources;
using static System.ComponentModel.EditorBrowsableState;

namespace Tiger.AppConfig;

/// <summary>A strongly typed resource class for looking up localized strings, etc.</summary>
static class Resources
{
    static ResourceManager? s_resourceManager;
    static object s_resourceManagerLock = new();

    /// <summary>Gets the cached <see cref="ResourceManager"/> instance used by this class.</summary>
    [EditorBrowsable(Advanced)]
    public static ResourceManager ResourceManager => LazyInitializer.EnsureInitialized(
        ref s_resourceManager,
        ref s_resourceManagerLock,
        () => new ResourceManager("Tiger.AppConfig.Resources", typeof(Resources).Assembly));

    /// <summary>Gets the standard log message prefix.</summary>
    public static string Preamble => ResourceManager.GetString(nameof(Preamble), null)!;

    /// <summary>Gets the timeout message.</summary>
    public static string TimedOut => ResourceManager.GetString(nameof(TimedOut), null)!;

    /// <summary>Gets the request failure message.</summary>
    public static string FailedToContact =>
        ResourceManager.GetString(nameof(FailedToContact) + "WithStatusCode", null)!;

    /// <summary>Gets the request cancellation message.</summary>
    public static string Canceled => ResourceManager.GetString(nameof(Canceled), null)!;

    /// <summary>Gets the message indicating malformed JSON.</summary>
    public static string DeserializationFailed => ResourceManager.GetString(nameof(DeserializationFailed), null)!;

    /// <summary>Gets the message indicating invalid JSON.</summary>
    public static string NotObject => ResourceManager.GetString(nameof(NotObject), null)!;

    /// <summary>Gets the suffix describing the user of a potentially stale cache.</summary>
    public static string UsingCache => ResourceManager.GetString(nameof(UsingCache), null)!;
}
