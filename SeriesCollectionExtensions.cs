using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {


    /// <summary>
    /// Extension methods for SeriesCollections
    /// </summary>
    public static class SeriesCollectionExtensions {

        /// <summary>
        /// Adds multiple series to a SeriesCollection
        /// </summary>
        /// <param name="col">SeriesCollection</param>
        /// <param name="series">IEnumerable of series</param>
        public static void Add(this SeriesCollection col, IEnumerable<Series> series) {
            foreach(Series ser in series)
                col.Add(ser);
        }
    }
}
