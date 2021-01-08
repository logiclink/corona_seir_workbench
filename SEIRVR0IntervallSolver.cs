using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogicLink.Corona {

    /// <summary>
    /// Solves the R₀ value of a SEIR model with a Levenberg-Marquardt method.
    /// </summary>
    /// <remarks>
    /// The <see cref="Solve(IProgress{int})"/> method iterates R₀ between 0 and 10 for every day of the SEIR model 
    /// and calculates the squares of the residuals between the confirmed cases and the case number od the SEIR model.
    /// See http://cow.physics.wisc.edu/~craigm/idl/Markwardt-MPFIT-Visualize2009.pdf for further details
    /// </remarks>
    public class SEIRVR0IntervalSolver : ISEIRVR0Solver {
        private readonly int _iInterval;

        /// <summary>
        /// Creates a new SEIRR0Solver object
        /// </summary>
        /// <param name="iInterval">Number of days from 1 to n for the interval in which R₀ should be calculated.</param>
        public SEIRVR0IntervalSolver(int iInterval = 1) => _iInterval = iInterval;

        #region ISEIRVR0Solver

        /// <summary>
        /// Gets or sets the SEIRV model
        /// </summary>
        public SEIRV SEIRV { get; set; }

        /// <summary>
        /// Gets or sets a list of confirmed cases which are compared to the calculated SEIR case numbers
        /// </summary>
        public List<int> Confirmed { get; set; }

        /// <summary>
        /// Gets or sets a list of vaccinated individuals. Must have the same number of elements as the <see cref="confirmed"/> list.
        /// </summary>
        public List<int> Vaccinated { get; set; }

        /// <summary>
        /// Solves the R₀ number for a given SEIR model by comparing calculated cases with a list of confiremd cases.
        /// </summary>
        /// <returns>An enumerable of R₀ values. The R₀ value is the largest R₀ with the smallest squared error.</returns>
        public IEnumerable<double> Solve(IProgress<int> p = null) {
            if(!(this.Confirmed?.Count > 0)) yield break;
            if(this.Confirmed.Count != this.Vaccinated.Count) throw new Exception("Number of elements in the list of confirmed cases is different to the number elements in the list of vaccinated individuals.");

            int iPCount = 0;
            SEIRV seirvCalc = new SEIRV(this.SEIRV);
            int iInterval = 0;
            double dR0Interval = 0d;
            for(int i = 0; i < this.Confirmed.Count; i++) {

                if(iInterval++ == 0)  { 
                    double dR0 = 0d;
                    double dResidual = double.MaxValue;
                    for(double d = 0.0d; d < 10d; d = Math.Round(d + 0.1d, 1)) {
                        ISEIRV seirvResiduals = new SEIRV(seirvCalc) { Reproduction = d };
                        double r = 0d; ;
                        for(int j = 1; j <= Math.Min(_iInterval, this.Confirmed.Count - i); j++) {
                            seirvResiduals.Vaccinated = this.Vaccinated[i + j - 1];
                            seirvResiduals.Calc(j);
                            r += Math.Pow(this.Confirmed[i + j - 1] - seirvResiduals.Exposed - seirvResiduals.Infectious - seirvResiduals.Removed, 2);
                        }
                        if(dResidual >= r) {
                            dR0 = d;
                            dResidual = r;
                        }
                    }
                    dR0Interval = dR0;
                }

                yield return dR0Interval;

                seirvCalc.Reproduction = dR0Interval;
                seirvCalc.Vaccinated = this.Vaccinated[i];
                seirvCalc.Calc(i);

                if(iInterval == _iInterval)
                    iInterval = 0;

                if(iPCount != 25 * i / this.Confirmed.Count) {
                    iPCount = 25 * i / this.Confirmed.Count;
                    p?.Report(4 * iPCount);
                }
            }
        }
        #endregion
    }


}
