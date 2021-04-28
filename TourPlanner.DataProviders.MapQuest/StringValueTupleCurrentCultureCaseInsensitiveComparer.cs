using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TourPlanner.DataProviders.MapQuest
{
    internal class StringValueTupleCurrentCultureCaseInsensitiveComparer : IEqualityComparer<(string, string)>
    {
        public bool Equals((string, string) x, (string, string) y)
            => StringComparer.CurrentCultureIgnoreCase.Equals(x.Item1, y.Item1)
                && StringComparer.CurrentCultureIgnoreCase.Equals(x.Item2, y.Item2);

        public int GetHashCode([DisallowNull] (string, string) obj)
            => HashCode.Combine(obj);
    }
}
