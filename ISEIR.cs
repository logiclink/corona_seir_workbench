using System;
using System.Collections.Generic;
using System.Text;

namespace LogicLink.Corona {

    /// <summary>
    /// Interface for SEIR model
    /// </summary>
    public interface ISEIR {

        #region SEIR Parameter Properties

        /// <summary>
        /// Timespan in which an individual is infected but not infectious
        /// </summary>
        TimeSpan IncubationPeriod { get; set; }

        /// <summary>
        /// Timespan in which an individual is infectious
        /// </summary>
        TimeSpan InfectiousPeriod { get; set;  }

        /// <summary>
        /// Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune. transl. "Basisreproduktionszahl"
        /// </summary>
        double Reproduction { get; set; }

        #endregion

        #region SEIR Properties

        /// <summary>
        /// Number of individuals in the S(usceptible) compartment
        /// </summary>
        int Susceptible { get; }

        /// <summary>
        /// Number of individuals in the E(xposed) compartment
        /// </summary>
        int Exposed { get; }

        /// <summary>
        /// Number of individuals in the I(nfectious) compartment
        /// </summary>
        int Infectious { get; }

        /// <summary>
        /// Number of individuals in the R(emoved) compartment
        /// </summary>
        int Removed { get; }

        #endregion

        #region SEIR Calculation Properties and Methods

        /// <summary>
        /// Day for which the numbers of individuals in all campartments were calculated
        /// </summary>
        int Day { get; }

        /// <summary>
        /// Calculates the number of individuals in all compartments for a day 
        /// </summary>
        /// <param name="iDay">Day to calculate. Has to be larger than <see cref="Day"/>.</param>
        void Calc(int iDay);

        #endregion
    }
}
