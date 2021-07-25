using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {

    /// <summary>
    /// Charting series view of RKI R₀-values from Nowcasting
    /// </summary>
    public class RKINowcastingDateSeriesView : IDateSeriesView {
        private readonly RKINowcasting _rnc;            // RKI Nowcasting-object

        private readonly Series _serReproduction7Day;   // Series of the average basic reproduction number (R₀) of the last 7 days

        /// <summary>
        /// Creates and initializes a new view object
        /// </summary>
        /// <param name="rnc">RKI Nowcasting-object</param>
        /// <param name="bReproduction">If true, R₀-series is shown for the second axis</param>
        /// <param name="bReproduction7Day">If true, 7 days average R₀-series is shown for the second axis</param>
        public RKINowcastingDateSeriesView(RKINowcasting rnc, bool bReproduction7Day = true) {
            _rnc = rnc;
            if(bReproduction7Day)
                _serReproduction7Day = new Series("Nowcasting 7-Day-R₀") { ChartType = SeriesChartType.Spline,
                                                                           YAxisType = AxisType.Secondary,
                                                                           XValueType = ChartValueType.Date,
                                                                           Color = Color.FromArgb(128, 128, 128),
                                                                           BorderDashStyle = ChartDashStyle.Dot,
                                                                           BorderWidth = 5 };
        }

        /// <summary>
        ///  Calculates the series with the data from the RKINowcasting-object for a time range
        /// </summary>
        /// <param name="dtStart">Start date of the time range</param>
        /// <param name="dtEnd">End date of teh time range</param>
        /// <param name="p">Optional progress object</param>
        /// <returns>Awaitable task.</returns>
        public async Task CalcAsync(DateTime dtStart, DateTime dtEnd, IProgress<int> p = null) {
            int iCount = 0;
            int iPCount = 0;
            await foreach(RKINowcasting.Record r in _rnc.GetDataAsync()) {
                if((r.Date >= dtStart) && 
                   (r.Date <= dtEnd)) {
                    if(_serReproduction7Day != null)
                        _serReproduction7Day.Points.AddXY(r.Date, r.Reproduction7Day);
                }
                if(iPCount != 25 * ++iCount / (dtEnd - dtStart).Days) {
                    iPCount = 25 * ++iCount / (dtEnd - dtStart).Days;
                    p?.Report(4 * iPCount);
                }
                
            }
        }

        #region IEnumerable Interface

        /// <summary>
        /// Enumerates charting series
        /// </summary>
        /// <returns>Enumerable of charting series</returns>
        public IEnumerator<Series> GetEnumerator() {
            if(_serReproduction7Day != null)
                yield return _serReproduction7Day;
        }

        /// <summary>
        /// Untyped version of the typed enumerator
        /// </summary>
        /// <returns>Enumerable of object</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
