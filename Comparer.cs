using System;
using System.Collections.Generic;

namespace LogicLink.Corona {

    /// <summary>
    /// Class for using anonymous functions in an <see cref="IComparer"/> interface
    /// </summary>
    /// <typeparam name="T">Type of objects to compare</typeparam>
    public class Comparer<T> : IComparer<T> {
        private readonly Func<T, T, int> _compare;
        public Comparer(Func<T, T, int> comparer) {
            _compare = comparer;
        }
        public int Compare(T x, T y) {
            return _compare(x, y);
        }
    }
}
