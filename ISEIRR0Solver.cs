using System;
using System.Collections.Generic;
using System.Text;

namespace LogicLink.Corona {

    // Interface of a solver for R₀ values using a SEIR model
    public interface ISEIRR0Solver : IR0Solver {

        /// <summary>
        /// Gets or sets the SEIR model
        /// </summary>
        SEIR SEIR { get; set; }

        /// <summary>
        /// Gets or sets a list of confirmed cases which are compared to the calculated SEIR case numbers
        /// </summary>
        List<int> Confirmed { get; set; }

    }
}
