using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {

    /// <summary>
    /// Charting series view of a SEIR object. X-axis is a range of day numbers.
    /// </summary>
    public class SEIRSeriesView : ISeriesView {
        private readonly ISEIR _seir;               // SEIR-object

        private readonly Series _serSusceptible;    // Series of the number of individuals in the S(usceptible) compartment
        private readonly Series _serExposed;        // Series of the number of individuals in the E(xposed) compartment
        private readonly Series _serInfectious;     // Series of the number of individuals in the I(nfectious) compartment
        private readonly Series _serRemoved;        // Series of the number of individuals in the R(emoved) compartment
        private readonly Series _serCases;          // Series of the total number of confirmed infected individuals ( Number of E(xposed) + I(nfectious) + R(emoved) )
        private readonly Series _serDaily;          // Series of the number of confirmed infected individuals for the day ( Cases - Cases of the previous day )
        private readonly Series _ser7Days;          // Series of the 7 day average of daily cases per 100.000 individuals
        private readonly Series _serReproduction;   // Series of the basic reproduction number (R₀)

        /// <summary>
        /// Creates and initializes a new view object
        /// </summary>
        /// <param name="seir">SEIR-object</param>
        /// <param name="bSusceptible">If true, susceptible-series is shown.</param>
        /// <param name="bExposed">>If true, exposed-series is shown.</param>
        /// <param name="bInfectious">If true, infectious-series is shown.</param>
        /// <param name="bRemoved">If true, removed-series is shown.</param>
        /// <param name="bCases">If true, total cases-series is shown.</param>
        /// <param name="bDaily">If true, daily cases-series is shown.</param>
        /// <param name="b7Days">If true, 7 day average of daily cases per 100.000-series is shown.</param>
        /// <param name="bReproduction">If true, R₀-series is shown for the second axis</param>
        public SEIRSeriesView(ISEIR seir, bool bSusceptible = true, bool bExposed = true, bool bInfectious = true, bool bRemoved = true, bool bCases = true, bool bDaily = true, bool b7Days = true, bool bReproduction = true) {
            _seir = seir;

            if(bSusceptible)
                _serSusceptible = new Series("Susceptible") { ChartType = SeriesChartType.Spline,
                                                              Color = Color.LightSkyBlue,
                                                              BorderWidth = 5 };

            if(bExposed)
                _serExposed = new Series("Exposed") { ChartType = SeriesChartType.Spline,
                                                      Color = Color.Orange,
                                                      BorderWidth = 5 };

            if(bInfectious)
                _serInfectious = new Series("Infectious") { ChartType = SeriesChartType.Spline,
                                                            Color = Color.Red,
                                                            BorderWidth = 5 };

            if(bRemoved)
                _serRemoved = new Series("Removed") { ChartType = SeriesChartType.Spline,
                                                      Color = Color.LimeGreen,
                                                      BorderWidth = 5 };

            if(bCases)
                _serCases = new Series("Cases") { ChartType = SeriesChartType.Spline,
                                                  Color = Color.Yellow,
                                                  BorderWidth = 5 };

            if(bDaily)
                _serDaily = new Series("Daily Cases") { ChartType = SeriesChartType.Column,
                                                        Color = Color.Yellow,
                                                        BorderWidth = 5 };

            if(b7Days)
                _ser7Days = new Series("7 Days Incidence") { ChartType = SeriesChartType.Column,
                                                             Color = Color.Goldenrod,
                                                             BorderWidth = 5 };

            if(bReproduction)
                _serReproduction = new Series("Reproduction") { ChartType = SeriesChartType.Spline,
                                                                YAxisType = AxisType.Secondary,
                                                                Color = Color.Black,
                                                                BorderWidth = 5 };
        }

        /// <summary>
        /// Calculates the series with the SEIR object for a number days
        /// </summary>
        /// <param name="iDays">Days for which the series should be calculated.</param>
        /// <returns>Awaitable task</returns>
        public async Task CalcAsync(int iDays) {
            int iPopulation = _seir.Susceptible + _seir.Exposed + _seir.Infectious + _seir.Removed;
            Queue<int> q7Days = new Queue<int>(7);
            for(int i = 1; i < iDays; i++) {
                int iDay = _seir.Day;
                int iCases = _seir.Exposed + _seir.Infectious + _seir.Removed;

                _seir.Calc(i);

                if(_serSusceptible != null)
                    _serSusceptible.Points.AddXY(_seir.Day, _seir.Susceptible);
                if(_serExposed != null)
                    _serExposed.Points.AddXY(_seir.Day, _seir.Exposed);
                if(_serInfectious != null)
                    _serInfectious.Points.AddXY(_seir.Day, _seir.Infectious);
                if(_serRemoved != null)
                    _serRemoved.Points.AddXY(_seir.Day, _seir.Removed);
                if(_serCases != null)
                    _serCases.Points.AddXY(_seir.Day, _seir.Exposed + _seir.Infectious + _seir.Removed);
                if(_serDaily != null)
                    _serDaily.Points.AddXY(_seir.Day, Math.Max(_seir.Exposed + _seir.Infectious + _seir.Removed - iCases, 0));

                if(_ser7Days != null) {
                    q7Days.Enqueue(Math.Max(_seir.Exposed + _seir.Infectious + _seir.Removed - iCases, 0) );
                    while(q7Days.Count > 7)
                        q7Days.Dequeue();
                    _ser7Days.Points.AddXY(_seir.Day, q7Days.Sum() / (iPopulation / 100000d));
                }

                if(_serReproduction != null)
                    _serReproduction.Points.AddXY(_seir.Day, _seir.Reproduction);
            }
            await Task.CompletedTask;
        }

        #region IEnumerable Interface

        /// <summary>
        /// Enumerates charting series
        /// </summary>
        /// <returns>Enumerable of charting series</returns>
        public IEnumerator<Series> GetEnumerator() {
            if(_serSusceptible != null)
                yield return _serSusceptible;
            if(_serExposed != null)
                yield return _serExposed;
            if(_serInfectious != null)
                yield return _serInfectious;
            if(_serRemoved != null)
                yield return _serRemoved;
            if(_serCases != null)
                yield return _serCases;
            if(_serDaily != null)
                yield return _serDaily;
            if(_ser7Days != null)
                yield return _ser7Days;
            if(_serReproduction != null)
                yield return _serReproduction;
        }

        /// <summary>
        /// Untyped version of the typed enumerator
        /// </summary>
        /// <returns>Enumerable of object</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
