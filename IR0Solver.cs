using System;
using System.Collections.Generic;
using System.Text;

namespace LogicLink.Corona {

    // Interface of a solver for R₀ values
    public interface IR0Solver {

        /// <summary>
        /// Calculates an enumerable of R₀ values for each day
        /// </summary>
        /// <param name="p">Optional progress object</param>
        /// <returns>Enumerable of R₀ values</returns>
        IEnumerable<double> Solve(IProgress<int> p = null);
    }
}
