using System;
using System.Collections.Generic;
using System.Text;

namespace LogicLink.Corona {

    // Interface of a solver for R₀ values using a SEIR model
    public interface ISEIRVR0Solver : IR0Solver {

        /// <summary>
        /// Gets or sets the SEIRV model
        /// </summary>
        SEIRV SEIRV { get; set; }

        /// <summary>
        /// Gets or sets a list of confirmed cases which are compared to the calculated SEIR case numbers
        /// </summary>
        List<int> Confirmed { get; set; }

        /// <summary>
        /// Gets or sets a list of vaccinated individuals which are compared to the calculated SEIR case numbers
        /// </summary>
        List<double> Vaccinated { get; set; }

    }
}
