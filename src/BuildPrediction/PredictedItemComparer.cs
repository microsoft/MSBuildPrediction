// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Comparer class for <see cref="PredictedItem"/>.
    /// </summary>
    internal sealed class PredictedItemComparer : IEqualityComparer<PredictedItem>
    {
        private PredictedItemComparer()
        {
        }

        public static PredictedItemComparer Instance { get; } = new PredictedItemComparer();

        public bool Equals(PredictedItem x, PredictedItem y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.PredictedBy.Count == y.PredictedBy.Count
                && PathComparer.Instance.Equals(x.Path, y.Path)
                && x.PredictedBy.All(p => y.PredictedBy.Contains(p));
        }

        public int GetHashCode(PredictedItem obj)
            => PathComparer.Instance.GetHashCode(obj.Path)
                ^ (obj.PredictedBy.Count == 0 ? 0xAAAAAA : obj.PredictedBy.Count * 0xBB)
                ^ obj.PredictedBy.Aggregate(0, (current, s) => current ^ StringComparer.OrdinalIgnoreCase.GetHashCode(s));
    }
}