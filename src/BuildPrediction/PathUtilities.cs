// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System.IO;

    internal static class PathUtilities
    {
        private static readonly char _badDirectorySeparatorChar = Path.DirectorySeparatorChar == '\\' ? '/' : '\\';

        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            path = NormalizeDirectorySeparators(path);

            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : path;
        }

        public static string NormalizeDirectorySeparators(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return path.Replace(_badDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}
