using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {

    /// <summary>
    /// Charting series view of OWID data
    /// </summary>
    public class OWIDDateSeriesView: IDateSeriesView {

        private readonly OWID _owid;                    // OWID-object
        private readonly string _sCountry;              // Country which is viewed

        private readonly Series _serVaccinated;         // Series of the total number of vaccinated individuals
        private readonly Series _serDailyVaccinated;    // Series of the number of confirmed infected individuals for the day

        /// <summary>
        /// Creates and initializes a new view object
        /// </summary>
        /// <param name="owd">OWID-object</param>
        /// <param name="sCountry">Country which is viewed</param>
        /// <param name="bVaccinated">If true, vaccinated-series is shown.</param>
        /// <param name="bDailyVaccinated">If true, daily vaccinated-series is shown.</param>
        public OWIDDateSeriesView(OWID owd, string sCountry = null, bool bVaccinated = true, bool bDailyVaccinated = true) {
            _owid = owd;
            _sCountry = sCountry;

            if(bVaccinated)
                _serVaccinated = new Series("Confirmed Vaccinated") { ChartType = SeriesChartType.Spline,
                                                                                  XValueType = ChartValueType.Date,
                                                                                  Color = Color.DarkGreen,
                                                                                  BorderWidth = 5 };

            if(bDailyVaccinated)
                _serDailyVaccinated = new Series("Daily Confirmed Vaccinated") { ChartType = SeriesChartType.Column,
                                                                                             XValueType = ChartValueType.Date,
                                                                                             Color = Color.DarkGreen,
                                                                                             BorderWidth = 5  };
             
        }

        /// <summary>
        ///  Calculates the series with the data from the JHU-object for a time range
        /// </summary>
        /// <param name="dtStart">Start date of the time range</param>
        /// <param name="dtEnd">End date of teh time range</param>
        /// <param name="p">Optional progress object</param>
        /// <returns>Awaitable task.</returns>
        public async Task CalcAsync(DateTime dtStart, DateTime dtEnd, IProgress<int> p = null) {
            int iCount = 0;
            int iPCount = 0;
            await foreach(OWID.Record r in _owid.GetDataAsync(_sCountry)) {
                if((r.Date >= dtStart) && 
                   (r.Date <= dtEnd)) {
                    if(_serVaccinated != null)
                        _serVaccinated.Points.AddXY(r.Date, r.Vaccinated);
                    if(_serDailyVaccinated != null)
                        _serDailyVaccinated.Points.AddXY(r.Date, r.DailyVaccinated);
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
            if(_serVaccinated != null)
                yield return _serVaccinated;
            if(_serDailyVaccinated != null)
                yield return _serDailyVaccinated;
        }

        /// <summary>
        /// Untyped version of the typed enumerator
        /// </summary>
        /// <returns>Enumerable of object</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
