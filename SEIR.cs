using System;
using System.Text;

namespace LogicLink.Corona {

    /// <summary>
    /// SEIR model class
    /// </summary>
    public class SEIR : ISEIR {

        #region Private Static Compartment Functions

        /// <summary>
        /// Daily number of individuals moving from S(usceptible) to E(xposed) compartment
        /// </summary>
        /// <param name="dSusceptible">Number of individuals in the S(usceptible) compartment.</param>
        /// <param name="dInfectious">Number of individuals in the I(nfectious) compartment.</param>
        /// <param name="iPopulation">Total number of indivisuals in all compartments.</param>
        /// <param name="tsInfectiousPeriod">Timespan in which an individual is infectious.</param>
        /// <param name="dReproduction">Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune. transl. "Basisreproduktionszahl"</param>
        /// <returns>Number of individuals</returns>
        private static double dΔSusceptibleToExposed(double dSusceptible, double dInfectious, int iPopulation, TimeSpan tsInfectiousPeriod, double dReproduction) {
            return (dSusceptible / iPopulation) * (dReproduction / tsInfectiousPeriod.TotalDays) * dInfectious;
        }

        /// <summary>
        /// Daily number of individuals moving from E(xposed) to I(nfectious) compartment
        /// </summary>
        /// <param name="dExposed">Number of individuals in the E(xposed) compartment.</param>
        /// <param name="tsIncubationPeriod">Timespan in which an individual is infected but not infectious.</param>
        /// <returns>Number of individuals</returns>
        private static double dΔExposedToInfectious(double dExposed, TimeSpan tsIncubationPeriod) {
            return dExposed / tsIncubationPeriod.TotalDays;
        }

        /// <summary>
        /// Daily number of individuals moving from I(nfectious) to R(emoved) compartment
        /// </summary>
        /// <param name="dInfectious">Number of individuals in the I(nfectious) compartment.</param>
        /// <param name="tsInfectiousPeriod">Timespan in which an individual is infectious.</param>
        /// <returns>Number of individuals</returns>
        private static double dΔInfectiousToRemoved(double dInfectious, TimeSpan tsInfectiousPeriod) {
            return dInfectious / tsInfectiousPeriod.TotalDays;
        }

        #endregion

        #region Private Variables
        private int _iPopulation;           // Total number of indivisuals in all compartmens

        private double _dSusceptible;       // Number of individuals in the S(usceptible) compartment
        private double _dExposed;           // Number of individuals in the E(xposed) compartment
        private double _dInfectious;        // Number of individuals in the I(nfectious) compartment
        private double _dRemoved;           // Number of individuals in the R(emoved) compartment

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Creates a new SEIR object
        /// </summary>
        /// <param name="iSusceptible">Number of individuals in the S(usceptible) compartment.</param>
        /// <param name="iInfectious">Number of individuals in the I(nfectious) compartment.</param>
        /// <param name="tsIncubationPeriod">Timespan in which an individual is infected but not infectious</param>
        /// <param name="tsInfectiousPeriod">Timespan in which an individual is infectious.</param>
        /// <param name="dReproduction">Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune. transl. "Basisreproduktionszahl"</param>
        /// <remarks>
        /// The population is the total number of all individuals in all compartments. In this case it's S(usceptible) + I(nfectious). 
        /// </remarks>
        public SEIR(int iSusceptible, int iInfectious, TimeSpan tsIncubationPeriod, TimeSpan tsInfectiousPeriod, double dReproduction) {
            _iPopulation = iSusceptible + iInfectious;

            _dSusceptible = iSusceptible;
            _dInfectious = iInfectious;

            this.IncubationPeriod = tsIncubationPeriod;
            this.InfectiousPeriod = tsInfectiousPeriod;
            this.Reproduction = dReproduction;
        }

        /// <summary>
        /// Creates a new SEIR object from the values of another SEIR object
        /// </summary>
        /// <param name="iSusceptible">Number of individuals in the S(usceptible) compartment.</param>
        /// <param name="iExposed">Number of individuals in the E(xposed) compartment.</param>
        /// <param name="iInfectious">Number of individuals in the I(nfectious) compartment.</param>
        /// <param name="iRemoved">Number of individuals in the R(emoved) compartment.</param>
        /// <param name="tsIncubationPeriod">Timespan in which an individual is infected but not infectious</param>
        /// <param name="tsInfectiousPeriod">Timespan in which an individual is infectious.</param>
        /// <param name="dReproduction">Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune. transl. "Basisreproduktionszahl"</param>
        /// <remarks>
        /// The population is the total number of all individuals in all compartments. In this case it's S(usceptible) + E(xposed) + I(nfectious) +R(emoved). 
        /// </remarks>
        public SEIR(int iSusceptible, int iExposed, int iInfectious, int iRemoved, TimeSpan tsIncubationPeriod, TimeSpan tsInfectiousPeriod, double dReproduction) : this(iSusceptible, iInfectious, tsIncubationPeriod, tsInfectiousPeriod, dReproduction)  {
            _iPopulation += iExposed + iRemoved;

            _dExposed = iExposed;
            _dRemoved = iRemoved;
        }

        /// <summary>
        /// Creates a new SEIR object from another ISEIR object
        /// </summary>
        /// <param name="seir">Another ISEIR objext</param>
        public SEIR(ISEIR seir) : this(seir.Susceptible, seir.Exposed, seir.Infectious, seir.Removed, seir.IncubationPeriod, seir.InfectiousPeriod, seir.Reproduction) { }

        #endregion

        #region Public SEIR Parameter Properties

        /// <summary>
        /// Timespan in which an individual is infected but not infectious
        /// </summary>
        public TimeSpan IncubationPeriod { get; set; }

        /// <summary>
        /// Timespan in which an individual is infectious
        /// </summary>
        public TimeSpan InfectiousPeriod { get; set; }

        /// <summary>
        /// Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune. transl. "Basisreproduktionszahl"
        /// </summary>
        public double Reproduction { get; set; }

        #endregion

        #region Public SEIR Properties

        /// <summary>
        /// Number of individuals in the S(usceptible) compartment.
        /// </summary>
        public int Susceptible => (int)Math.Round(_dSusceptible, 0);

        /// <summary>
        /// Number of individuals in the E(xposed) compartment.
        /// </summary>
        public int Exposed => (int)Math.Round(_dExposed, 0);

        /// <summary>
        /// Number of individuals in the I(nfectious) compartment
        /// </summary>
        public int Infectious => (int)Math.Round(_dInfectious, 0);

        /// <summary>
        /// Number of individuals in the R(emoved) compartment
        /// </summary>
        public int Removed => (int)Math.Round(_dRemoved, 0);

        #endregion

        #region Public SEIR Calculation Properties

        /// <summary>
        /// Day for which the numbers of individuals in all campartments were calculated
        /// </summary>
        public int Day { get; private set; } = 0;

        #endregion

        #region Public SEIR Calculation Methods

        /// <summary>
        /// Calculates the number of individuals in all compartments for a day 
        /// </summary>
        /// <param name="iDay">Day to calculate. Has to be larger than <see cref="Day"/>.</param>
        public void Calc(int iDay) {
            for(int i = this.Day; i < iDay; i++) {
                double dSusceptible = _dSusceptible;
                double dExposed = _dExposed;
                double dInfectious =_dInfectious;

                _dSusceptible -= dΔSusceptibleToExposed(dSusceptible, dInfectious, _iPopulation, this.InfectiousPeriod, this.Reproduction);
                _dExposed += dΔSusceptibleToExposed(dSusceptible, dInfectious, _iPopulation, this.InfectiousPeriod, this.Reproduction) - dΔExposedToInfectious(dExposed, this.IncubationPeriod);
                _dInfectious += dΔExposedToInfectious(dExposed, this.IncubationPeriod) - dΔInfectiousToRemoved(dInfectious, this.InfectiousPeriod);
                _dRemoved += dΔInfectiousToRemoved(dInfectious, this.InfectiousPeriod);
                this.Day++;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Text representation of the object.
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(1024);
            sb.AppendFormat("Day:\t{0}\n", this.Day);
            sb.AppendFormat("Susceptible:\t{0}\n", this.Susceptible);
            sb.AppendFormat("Exposed:\t{0}\n", this.Exposed);
            sb.AppendFormat("Infectious:\t{0}\n", this.Infectious);
            sb.AppendFormat("Removed:\t{0}\n", this.Removed);
            return sb.ToString();
        }

        #endregion
    }
}

