using System;
using System.Collections.Generic;
using System.Text;

namespace LogicLink.Corona {

    /// <summary>
    /// Extension methods for ReadOnlySpans
    /// </summary>
    public static class ReadOnlySpanExtensions {

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified char in the current span.
        /// Sequences within double quotes in the span are ignored.
        /// </summary>
        /// <param name="s">The span to search.</param>
        /// <param name="c">The char to search for.</param>
        /// <returns>The index of the occurrence of the char in the span. If not found, returns -1.</returns>
        public static int QuotedIndexOf(this ReadOnlySpan<char> s, char c) {
            bool bQuote = false;
            for(int i = 0; i < s.Length; i++) {
                if(s[i] == '"')
                    bQuote = !bQuote;
                else if(s[i] == c && !bQuote)
                    return i;
            }
            return -1;
        }
    }
}
