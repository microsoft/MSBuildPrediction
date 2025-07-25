// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET472

using System;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Contains extension methods for .NET Framework to match the (larger) .NET Core API.
    /// </summary>
    internal static class Polyfills
    {
        public static int IndexOf(this string str, char ch, StringComparison comparison)
        {
            if (comparison == StringComparison.Ordinal)
            {
                return str.IndexOf(ch);
            }
            else
            {
                throw new NotImplementedException("This part of the polyfill is not implemented");
            }
        }

        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            if (comparison == StringComparison.Ordinal)
            {
                return str.Replace(oldValue, newValue);
            }
            else
            {
                throw new NotImplementedException("This part of the polyfill is not implemented");
            }
        }

        public static bool Contains(this string str, string value, StringComparison comparison)
        {
            if (comparison == StringComparison.Ordinal)
            {
                return str.Contains(value);
            }
            else
            {
                throw new NotImplementedException("This part of the polyfill is not implemented");
            }
        }
    }
}
#endif