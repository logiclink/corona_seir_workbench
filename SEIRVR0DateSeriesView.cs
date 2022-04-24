using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {

    /// <summary>
    /// Charting series view of a SEIRV object and R₀-values. X-axis is a time range.
    /// </summary>
    public class SEIRVR0DateSeriesView : SEIRR0DateSeriesView {
        protected readonly Dictionary<DateTime, double> _dicVaccinated; // Dictionary of vaccinations

        private readonly Series _serVaccinated;                         // Series of the number of individuals in the V(accinated) compartment
        private readonly Series _serDailyVaccinated;                    // Series of the number of vaccinated individuals for the day ( Vaccinated - Vaccinated of the previous day )

        /// <summary>
        /// Creates and initializes a new view object
        /// </summary>
        /// <param name="seir">SEIR-object</param>
        /// <param name="dicReproduction">Dictionary of basic reproduction numbers (R₀). For missing dates the initial R₀ of the SEIR object is used.</param>
        /// <param name="bSusceptible">If true, susceptible-series is shown.</param>
        /// <param name="bExposed">>If true, exposed-series is shown.</param>
        /// <param name="bInfectious">If true, infectious-series is shown.</param>
        /// <param name="bRemoved">If true, removed-series is shown.</param>
        /// <param name="bCases">If true, total cases-series is shown.</param>
        /// <param name="bDaily">If true, daily cases-series is shown.</param>
        /// <param name="b7Days">If true, 7 day average of daily cases per 100.000-series is shown.</param>
        /// <param name="bReproduction">If true, R₀-series is shown for the second axis</param>
        /// <param name="bDoubledMarker">If true, double value diamond markers are added to cases-, daily- and 7days-series</param>
        /// <param name="bVaccinated">If true, vaccinated-series is shown</param>
        /// <param name="bDailyVaccinated">If true, daily vaccinated-series is shown</param>
        public SEIRVR0DateSeriesView(ISEIRV seirv, Dictionary<DateTime, double> dicReproduction, Dictionary<DateTime, double> dicVaccinated, bool bSusceptible = true, bool bExposed = true, bool bInfectious = true, bool bRemoved = true, bool bCases = true, bool bDaily = true, bool b7Days = true, bool bReproduction = true, bool bDoubledMarker = true, bool bVaccinated = true, bool bDailyVaccinated = true) : base(seirv, dicReproduction, bSusceptible, bExposed, bInfectious, bRemoved, bCases, bDaily, b7Days, bReproduction, bDoubledMarker) {
            _dicVaccinated = dicVaccinated;
            if(bVaccinated)
                _serVaccinated = new Series("Vaccinated") { ChartType = SeriesChartType.Spline,
                                                            XValueType = ChartValueType.Date,
                                                            Color = Color.PaleGreen,
                                                            BorderWidth = 5 };

            if(bDailyVaccinated)
                _serDailyVaccinated = new Series("Daily Vaccinated") { ChartType = SeriesChartType.Column,
                                                                       XValueType = ChartValueType.Date,
                                                                       Color = Color.PaleGreen,
                                                                       BorderWidth = 5 };        
        }

        /// <summary>
        ///  Calculates the series with the SEIR object for a time range with R₀-values of a dictionary.
        /// </summary>
        /// <param name="dtStart">Start date of the time range</param>
        /// <param name="dtEnd">End date of teh time range</param>
        /// <param name="p">Optional progress object</param>
        /// <returns>Awaitable task.</returns>
        public override async Task CalcAsync(DateTime dtStart, DateTime dtEnd, IProgress<int> p = null) {
            double dReproduction = _seir.Reproduction;
            int iPCount = 0;
            int iTotalDays = (dtEnd - dtStart).Days;
            int iPopulation = _seir.Susceptible + _seir.Exposed + _seir.Infectious + _seir.Removed;
            Queue<int> q7Days = new Queue<int>(7);
            int iCasesToday = 0;
            int iDailyToday = 0;
            int i7DaysToday = 0;
            for(DateTime dt = dtStart.AddDays(1d); dt <= dtEnd; dt = dt.AddDays(1d)) {
                int iCases = _seir.Exposed + _seir.Infectious + _seir.Removed;
                double dVaccinated = ((ISEIRV)_seir).Vaccinated;
                int iDays = (dt - dtStart).Days;

                _seir.Reproduction = _dicReproduction.TryGetValue(dt, out double d) ? d : dReproduction;
                ((ISEIRV)_seir).Vaccinated = _dicVaccinated.TryGetValue(dt, out double j) ? j : ((ISEIRV)_seir).Vaccinated;

                _seir.Calc(iDays);

                if(_serSusceptible != null)
                    _serSusceptible.Points.AddXY(dt, _seir.Susceptible - ((ISEIRV)_seir).Vaccinated * ((ISEIRV)_seir).Effectiveness);
                if(_serExposed != null)
                    _serExposed.Points.AddXY(dt, _seir.Exposed);
                if(_serInfectious != null)
                    _serInfectious.Points.AddXY(dt, _seir.Infectious);
                if(_serRemoved != null)
                    _serRemoved.Points.AddXY(dt, _seir.Removed);
                if(_serCases != null) {
                    int i = _serCases.Points.AddXY(dt, _seir.Exposed + _seir.Infectious + _seir.Removed);
                    if(_bDoubledMarker && iCasesToday != 0 && iCasesToday * 2 <_seir.Exposed + _seir.Infectious + _seir.Removed) {
                        iCasesToday = 0;
                        _serCases.Points[i].MarkerStyle = MarkerStyle.Diamond;
                        _serCases.Points[i].MarkerSize = _serCases.BorderWidth * 2;
                        _serCases.Points[i].MarkerBorderColor = Color.DarkGray;
                    }
                }
                if(_serDaily != null) {
                    int i = _serDaily.Points.AddXY(dt, Math.Max(_seir.Exposed + _seir.Infectious + _seir.Removed - iCases, 0));  // Remarks: Rounding errors might lead to -1 as value. Thus, Math.Max([Cases], 0) is used
                    if(_bDoubledMarker && iDailyToday != 0 && iDailyToday * 2 < Math.Max(_seir.Exposed + _seir.Infectious + _seir.Removed - iCases, 0)) {
                        iDailyToday = 0;
                        _serDaily.Points[i].MarkerStyle = MarkerStyle.Diamond;
                        _serDaily.Points[i].MarkerSize = _serDaily.BorderWidth * 2;
                        _serDaily.Points[i].MarkerBorderColor = Color.DarkGray;
                    }
                }

                if(_ser7Days != null) {
                    q7Days.Enqueue(Math.Max(_seir.Exposed + _seir.Infectious + _seir.Removed - iCases, 0));
                    while(q7Days.Count > 7)
                        q7Days.Dequeue();
                    int i = _ser7Days.Points.AddXY(dt, Math.Round(q7Days.Sum() / (iPopulation / 100000d), 2));
                    if(_bDoubledMarker && i7DaysToday != 0 && i7DaysToday * 2 < q7Days.Sum()) {
                        i7DaysToday = 0;
                        _ser7Days.Points[i].MarkerStyle = MarkerStyle.Diamond;
                        _ser7Days.Points[i].MarkerSize = _ser7Days.BorderWidth * 2;
                        _ser7Days.Points[i].MarkerBorderColor = Color.DarkGray;
                    }
                }

                if(_serReproduction != null)
                    _serReproduction.Points.AddXY(dt, _seir.Reproduction);

                if(_serVaccinated != null)
                    _serVaccinated.Points.AddXY(dt, ((ISEIRV)_seir).Vaccinated);

                if(_serDailyVaccinated != null)
                    _serDailyVaccinated.Points.AddXY(dt, Math.Max(((ISEIRV)_seir).Vaccinated - dVaccinated, 0));

                if(_bDoubledMarker && dt == DateTime.Today) {
                    iCasesToday = _seir.Exposed + _seir.Infectious + _seir.Removed;
                    iDailyToday = Math.Max(_seir.Exposed + _seir.Infectious + _seir.Removed - iCases, 0);
                    i7DaysToday = q7Days.Sum();
                }

                if(iPCount != 25 * iDays / iTotalDays) {
                    iPCount = 25 * iDays / iTotalDays;
                    p?.Report(4 * iPCount);
                }
            }
            await Task.CompletedTask;
        }

        #region IEnumerable Interface

        /// <summary>
        /// Enumerates charting series
        /// </summary>
        /// <returns>Enumerable of charting series</returns>
        public override IEnumerator<Series> GetEnumerator() {
            IEnumerator<Series> e = base.GetEnumerator();
            while(e.MoveNext())
              yield return e.Current;

            if(_serVaccinated != null)
                yield return _serVaccinated;
            if(_serDailyVaccinated != null)
                yield return _serDailyVaccinated;
        }

        #endregion
    }
}
