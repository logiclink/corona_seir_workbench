using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {

    /// <summary>
    /// Charting series view of JHU data
    /// </summary>
    public class JHUDateSeriesView : IDateSeriesView {

        private readonly JHU _jhu;                  // JHU-object
        private readonly string _sCountry;          // Country which is viewed
        private readonly int _iPopulation;          // Initial population for the calculation of the 7 day incidence value per 100.000 individuals

        private readonly Series _serConfirmed;      // Series of the total number of confirmed infected individuals
        private readonly Series _serDailyConfirmed; // Series of the number of confirmed infected individuals for the day
        private readonly Series _ser7DaysConfirmed; // Series of the 7 day average of daily cases per 100.000 individuals
        private readonly Series _serRecovered;      // Series of the total number of recovered individuals
        private readonly Series _serDeaths;         // Series of the total number of deaths of infected individuals

        /// <summary>
        /// Creates and initializes a new view object
        /// </summary>
        /// <param name="jhu">JHU-object</param>
        /// <param name="sCountry">Country which is viewed</param>
        /// <param name="iPopulation">Initial population for the calculation of the 7 day incidence value per 100.000 individuals</param>
        /// <param name="bConfirmed">If true, confirmed-series is shown.</param>
        /// <param name="bDailyConfirmed">If true, daily confirmed cases-series is shown.</param>
        /// <param name="b7DaysConfirmed">If true, 7 day average of daily confirmed cases per 100.000-series is shown.</param>
        /// <param name="bRecovered">If true, total recovered-series is shown.</param>
        /// <param name="bDeaths">If true, total death-series is shown.</param>
        public JHUDateSeriesView(JHU jhu, string sCountry = null, int iPopulation = 0, bool bConfirmed = true, bool bDailyConfirmed = true, bool b7DaysConfirmed = true, bool bRecovered = true, bool bDeaths = true) {
            _jhu = jhu;
            _sCountry = sCountry;
            _iPopulation = iPopulation;

            if(bConfirmed)
                _serConfirmed = new Series("Confirmed") { ChartType = SeriesChartType.Spline,
                                                          XValueType = ChartValueType.Date,
                                                          Color = Color.Crimson,
                                                          BorderWidth = 5 };

            if(bDailyConfirmed)
                _serDailyConfirmed = new Series("Daily Confirmed") { ChartType = SeriesChartType.Column,
                                                                     XValueType = ChartValueType.Date,
                                                                     Color = Color.Crimson,
                                                                     BorderWidth = 5  };

            if(b7DaysConfirmed)
                _ser7DaysConfirmed = new Series("7 Days Confirmed") { ChartType = SeriesChartType.Column,
                                                                     XValueType = ChartValueType.Date,
                                                                     Color = Color.DarkRed,
                                                                     BorderWidth = 5  };

            if(bRecovered)
                _serRecovered = new Series("Recovered") { ChartType = SeriesChartType.Spline,
                                                          XValueType = ChartValueType.Date,
                                                          Color = Color.Blue,
                                                          BorderWidth = 5 };

            if(bDeaths)
                _serDeaths = new Series("Deaths") { ChartType = SeriesChartType.Spline,
                                                    XValueType = ChartValueType.Date,
                                                    Color = Color.Black,
                                                    BorderWidth = 5 };
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
            Queue<int> q7Days = new Queue<int>(7);
            await foreach(JHU.Record r in _jhu.GetDataAsync(_sCountry)) {
                if((r.Date >= dtStart) && 
                   (r.Date <= dtEnd)) {
                    if(_serConfirmed != null)
                        _serConfirmed.Points.AddXY(r.Date, r.Confirmed);
                    if(_serDailyConfirmed != null)
                        _serDailyConfirmed.Points.AddXY(r.Date, r.DailyConfirmed);

                    if(_ser7DaysConfirmed != null && _iPopulation != 0) {
                        q7Days.Enqueue(r.DailyConfirmed);
                        while(q7Days.Count > 7)
                            q7Days.Dequeue();
                        _ser7DaysConfirmed.Points.AddXY(r.Date, Math.Round(q7Days.Sum() / (_iPopulation / 100000d), 2));
                    }

                    if(_serRecovered != null)
                        _serRecovered.Points.AddXY(r.Date, r.Recovered);
                    if(_serDeaths != null)
                        _serDeaths.Points.AddXY(r.Date, r.Deaths);
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
            if(_serConfirmed != null)
                yield return _serConfirmed;
            if(_serDailyConfirmed != null)
                yield return _serDailyConfirmed;
            if(_ser7DaysConfirmed != null)
                yield return _ser7DaysConfirmed;
            if(_serRecovered != null)
                yield return _serRecovered;
            if(_serDeaths != null)
                yield return _serDeaths;
        }

        /// <summary>
        /// Untyped version of the typed enumerator
        /// </summary>
        /// <returns>Enumerable of object</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
