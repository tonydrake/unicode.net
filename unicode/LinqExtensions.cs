﻿using System.Collections.Generic;
using System.Linq;

namespace NeoSmart.Unicode
{
    public static class LinqExtensions
    {
        public static bool In<T>(this T t, IEnumerable<T> collection)
        {
            return collection.Contains(t);
        }
    }
}
