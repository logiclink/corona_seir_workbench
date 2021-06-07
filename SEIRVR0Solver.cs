using System;
using System.Collections.Generic;

namespace LogicLink.Corona {

    /// <summary>
    /// Solves the R₀ value of a SEIR model with a Levenberg-Marquardt method.
    /// </summary>
    /// <remarks>
    /// The <see cref="Solve(IProgress{int})"/> method iterates R₀ between 0 and 10 for every day of the SEIR model 
    /// and calculates the squares of the residuals between the confirmed cases and the case number of the SEIR model.
    /// See http://cow.physics.wisc.edu/~craigm/idl/Markwardt-MPFIT-Visualize2009.pdf for further details
    /// </remarks>
    public class SEIRVR0Solver : ISEIRVR0Solver {
        private readonly int _iResidualDayWindow;

        /// <summary>
        /// Creates a new SEIRR0Solver object
        /// </summary>
        /// <param name="iResidualDayWindow">Number of days from 1 to n for residuals with which R₀ should be calculated.</param>
        public SEIRVR0Solver(int iResidualDayWindow = 1) => _iResidualDayWindow = iResidualDayWindow;

        #region ISEIRVR0Solver

        /// <summary>
        /// Gets or sets the SEIRV model
        /// </summary>
        public SEIRV SEIRV { get; set; }

        /// <summary>
        /// Gets or sets a list of confirmed cases which are compared to the calculated SEIRV case numbers
        /// </summary>
        public List<int> Confirmed { get; set; }

        /// <summary>
        /// Gets or sets a list of vaccinated individuals. Must have the same number of elements as the <see cref="confirmed"/> list.
        /// </summary>
        public List<double> Vaccinated { get; set; }

        /// <summary>
        /// Solves the R₀ number for a given SEIRV model by comparing calculated cases with a list of confiremd cases.
        /// The number of vaccinated individuals from the <see cref="Vaccinated"/> list is used.
        /// </summary>
        /// <returns>An enumerable of R₀ values. The R₀ value is the largest R₀ with the smallest squared error.</returns>
        public IEnumerable<double> Solve(IProgress<int> p = null) {
            if(!(this.Confirmed?.Count > 0)) yield break;
            if(this.Confirmed.Count != this.Vaccinated.Count) throw new Exception("Number of elements in the list of confirmed cases is different to the number elements in the list of vaccinated individuals.");

            int iPCount = 0;
            double dR0 = 0d;
            SEIRV seirvCalc = new SEIRV(this.SEIRV);

            // Calculate R₀ from the start to the last possible residual window
            for(int i = 0; i < this.Confirmed.Count - _iResidualDayWindow + 1; i++) {

                dR0 = 0d;
                double dResidual = double.MaxValue;
                for(double d = 0d; d < 10d; d = Math.Round(d + 0.1d, 1)) {
                    ISEIRV seirvResiduals = new SEIRV(seirvCalc) { Reproduction = d };
                    double r = 0d; ;
                    for(int j = 1; j <= _iResidualDayWindow; j++) {
                        seirvResiduals.Vaccinated = this.Vaccinated[i + j - 1];
                        seirvResiduals.Calc(j);
                        r += Math.Pow(this.Confirmed[i + j - 1] - seirvResiduals.Exposed - seirvResiduals.Infectious - seirvResiduals.Removed, 2);
                    }
                    if(dResidual >= r) {
                        dR0 = d;
                        dResidual = r;
                    }
                }

                yield return dR0;

                seirvCalc.Reproduction = dR0;
                seirvCalc.Vaccinated = this.Vaccinated[i];
                seirvCalc.Calc(i);
                
                if(iPCount != 25 * i / this.Confirmed.Count) {
                    iPCount = 25 * i / this.Confirmed.Count;
                    p?.Report(4 * iPCount);
                }
            }

            // Return R₀ for the days of the last residual window
            // Remarks: The last calculated R₀ value gives the smallest error for the days of the residual window.
            // Thus, this is the best guess of R₀ for these days. However, changes in R₀ in theses days are not considered.
            for(int i = this.Confirmed.Count - _iResidualDayWindow + 1; i < this.Confirmed.Count; i++) {
                yield return dR0;

                if(iPCount != 25 * i / this.Confirmed.Count) {
                    iPCount = 25 * i / this.Confirmed.Count;
                    p?.Report(4 * iPCount);
                }
            }
        }
        #endregion
    }


}
