using System;
using System.Collections.Generic;

namespace LogicLink.Corona {

    /// <summary>
    /// Solves the R₀ value of a SEIR model with a Levenberg-Marquardt method.
    /// </summary>
    /// <remarks>
    /// The <see cref="Solve(IProgress{int})"/> method iterates R₀ between 0 and 10 for every day of the SEIR model 
    /// and calculates the squares of the residuals between the confirmed cases and the case number od the SEIR model.
    /// See http://cow.physics.wisc.edu/~craigm/idl/Markwardt-MPFIT-Visualize2009.pdf for further details
    /// </remarks>
    public class SEIRR0Solver : ISEIRR0Solver {
        private readonly int _iResidualDayWindow;

        /// <summary>
        /// Creates a new SEIRR0Solver object
        /// </summary>
        /// <param name="iResidualDayWindow">Number of days from 1 to n for residuals with which R₀ should be calculated.</param>
        public SEIRR0Solver(int iResidualDayWindow = 1) => _iResidualDayWindow = iResidualDayWindow;

        #region ISEIRR0Solver

        /// <summary>
        /// Gets or sets the SEIR model
        /// </summary>
        public SEIR SEIR { get; set; }

        /// <summary>
        /// Gets or sets a list of confirmed cases which are compared to the calculated SEIR case numbers
        /// </summary>
        public List<int> Confirmed { get; set; }

        /// <summary>
        /// Solves the R₀ number for a given SEIR model by comparing calculated cases with a list of confiremd cases.
        /// </summary>
        /// <returns>An enumerable of R₀ values. The R₀ value is the largest R₀ with the smallest squared error.</returns>
        public IEnumerable<double> Solve(IProgress<int> p = null) {
            int iPCount = 0;
            SEIR seirCalc = new SEIR(this.SEIR);
            for(int i = 0; i < this.Confirmed.Count; i++) {
                double dR0 = 0d;
                double dResidual = double.MaxValue;
                for(double d = 0.0d; d < 10d; d = Math.Round(d + 0.1d, 1)) {
                    ISEIR seirResiduals = new SEIR(seirCalc) { Reproduction = d };
                    double r = 0d; ;
                    for(int j = 1; j <= Math.Min(_iResidualDayWindow, this.Confirmed.Count - i); j++) {
                        seirResiduals.Calc(j);
                        r += Math.Pow(this.Confirmed[i + j - 1] - seirResiduals.Exposed - seirResiduals.Infectious - seirResiduals.Removed, 2);
                    }
                    if(dResidual >= r) {
                        dR0 = d;
                        dResidual = r;
                    }
                }

                yield return dR0;

                seirCalc.Reproduction = dR0;
                seirCalc.Calc(i);
                
                if(iPCount != 25 * i / this.Confirmed.Count) {
                    iPCount = 25 * i / this.Confirmed.Count;
                    p?.Report(4 * iPCount);
                }
            }
        }
        #endregion
    }


}
