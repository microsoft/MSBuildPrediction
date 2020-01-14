// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    internal readonly struct ValueAndTypeName<T>
    {
        public readonly T Value;

        /// <summary>
        /// Cached type name - we expect object instances to be reused many times in
        /// an overall execution, avoid doing the reflection over and over in
        /// to get the type name.
        /// </summary>
        public readonly string TypeName;

        public ValueAndTypeName(T value)
        {
            Value = value;
            TypeName = value.GetType().Name;
        }
    }
}
