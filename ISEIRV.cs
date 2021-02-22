using System;
using System.Collections.Generic;
using System.Text;

namespace LogicLink.Corona {

    /// <summary>
    /// Interface for SEIR model with vaccination.
    /// </summary>
    public interface ISEIRV : ISEIR {

        #region SEIRV Parameter Properties

        /// <summary>
        /// Effectiveness of a vaccine ranging from 0 to 1. 1 stands for 100% of protection against an infection.
        /// </summary>
        double Effectiveness { get; }

        /// <summary>
        /// TimeSpan until a vaccine protects against an infection.
        /// </summary>
        public TimeSpan ProtectionStartPeriod { get; }

        #endregion

        #region SEIRV Properties

        /// <summary>
        /// Number of individuals in the V(accinated) compartment. This value is set manually.
        /// </summary>
        int Vaccinated { get; set; }

        #endregion

    }
}
