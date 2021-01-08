﻿using System;
using System.Text;

namespace LogicLink.Corona {

    /// <summary>
    /// SEIRV model class
    /// </summary>
    public class SEIRV : SEIR, ISEIRV {

        #region Protected Static Compartment Functions

        /// <summary>
        /// Daily number of individuals moving from S(usceptible) to E(xposed) compartment
        /// </summary>
        /// <param name="dSusceptible">Number of individuals in the S(usceptible) compartment.</param>
        /// <param name="dInfectious">Number of individuals in the I(nfectious) compartment.</param>
        /// <param name="iVaccinated">Number of individuals in the V(accinated) compartment.</param>
        /// <param name="iPopulation">Total number of indivisuals in all compartments.</param>
        /// <param name="tsInfectiousPeriod">Timespan in which an individual is infectious.</param>
        /// <param name="dReproduction">Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune.</param>
        /// <param name="dEffectiveness">Effectiveness of a vaccine ranging from 0 to 1. 1 stands for 100% of protection against an infection.</param>
        /// <returns>Number of individuals</returns>
        protected static double dΔSusceptibleToExposed(double dSusceptible, double dInfectious, int iVaccinated, int iPopulation, TimeSpan tsInfectiousPeriod, double dReproduction, double dEffectiveness) {
            return ((dSusceptible - iVaccinated * dEffectiveness) / iPopulation) * (dReproduction / tsInfectiousPeriod.TotalDays) * dInfectious;
        }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Creates a new SEIR object
        /// </summary>
        /// <param name="iSusceptible">Number of individuals in the S(usceptible) compartment.</param>
        /// <param name="iInfectious">Number of individuals in the I(nfectious) compartment.</param>
        /// <param name="tsIncubationPeriod">Timespan in which an individual is infected but not infectious</param>
        /// <param name="tsInfectiousPeriod">Timespan in which an individual is infectious.</param>
        /// <param name="dReproduction">Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune.</param>
        /// <remarks>
        /// The population is the total number of all individuals in all SEIR compartments. In this case it's S(usceptible) + I(nfectious). 
        /// </remarks>
        public SEIRV(int iSusceptible, int iInfectious, TimeSpan tsIncubationPeriod, TimeSpan tsInfectiousPeriod, double dReproduction, double dEffectiveness) : base(iSusceptible, iInfectious, tsIncubationPeriod, tsInfectiousPeriod, dReproduction) {
            this.Effectiveness = dEffectiveness;
        }

        /// <summary>
        /// Creates a new SEIR object
        /// </summary>
        /// <param name="iSusceptible">Number of individuals in the S(usceptible) compartment.</param>
        /// <param name="iInfectious">Number of individuals in the I(nfectious) compartment.</param>
        /// <param name="iVaccinated">Number of individuals in the V(accinated) compartment.</param>
        /// <param name="tsIncubationPeriod">Timespan in which an individual is infected but not infectious</param>
        /// <param name="tsInfectiousPeriod">Timespan in which an individual is infectious.</param>
        /// <param name="dReproduction">Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune.</param>
        /// <remarks>
        /// The population is the total number of all individuals in all SEIR compartments. In this case it's S(usceptible) + I(nfectious). 
        /// </remarks>
        public SEIRV(int iSusceptible, int iInfectious, int iVaccinated, TimeSpan tsIncubationPeriod, TimeSpan tsInfectiousPeriod, double dReproduction, double dEffectiveness) : this(iSusceptible, iInfectious, tsIncubationPeriod, tsInfectiousPeriod, dReproduction, dEffectiveness) {
            this.Vaccinated = iVaccinated;
        }

        /// <summary>
        /// Creates a new SEIR object from the values of another SEIR object
        /// </summary>
        /// <param name="iSusceptible">Number of individuals in the S(usceptible) compartment.</param>
        /// <param name="iExposed">Number of individuals in the E(xposed) compartment.</param>
        /// <param name="iInfectious">Number of individuals in the I(nfectious) compartment.</param>
        /// <param name="iRemoved">Number of individuals in the R(emoved) compartment.</param>
        /// <param name="iVaccinated">Number of individuals in the V(accinated) compartment.</param>
        /// <param name="tsIncubationPeriod">Timespan in which an individual is infected but not infectious</param>
        /// <param name="tsInfectiousPeriod">Timespan in which an individual is infectious.</param>
        /// <param name="dReproduction">Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune. transl. "Basisreproduktionszahl"</param>
        /// <remarks>
        /// The population is the total number of all individuals in all SEIR compartments. In this case it's S(usceptible) + E(xposed) + I(nfectious) +R(emoved). 
        /// </remarks>
        public SEIRV(int iSusceptible, int iExposed, int iInfectious, int iRemoved, int iVaccinated, TimeSpan tsIncubationPeriod, TimeSpan tsInfectiousPeriod, double dReproduction, double dEffectiveness) : base(iSusceptible, iExposed, iInfectious, iRemoved, tsIncubationPeriod, tsInfectiousPeriod, dReproduction)  {
            this.Vaccinated = iVaccinated;
            this.Effectiveness = dEffectiveness;
        }

        /// <summary>
        /// Creates a new SEIR object from another ISEIRV object
        /// </summary>
        /// <param name="seir">Another ISEIRV objext</param>
        public SEIRV(ISEIRV seirv) : this(seirv.Susceptible, seirv.Exposed, seirv.Infectious, seirv.Removed, seirv.Vaccinated, seirv.IncubationPeriod, seirv.InfectiousPeriod, seirv.Reproduction, seirv.Effectiveness) { }

        #endregion

        #region Public SEIRV Parameter Properties

        /// <summary>
        /// Effectiveness of a vaccine ranging from 0 to 1. 1 stands for 100% of protection against an infection.
        /// </summary>
        public double Effectiveness { get; private set; }

        #endregion

        #region Public SEIRV Properties

        /// <summary>
        /// Number of individuals in the V(accinated) compartment. This value is set manually.
        /// </summary>
        public int Vaccinated { get; set; }

        #endregion

        #region Public SEIR Calculation Methods

        /// <summary>
        /// Calculates the number of individuals in all compartments for a day 
        /// </summary>
        /// <param name="iDay">Day to calculate. Has to be larger than <see cref="Day"/>.</param>
        public override void Calc(int iDay) {
            for(int i = this.Day; i < iDay; i++) {
                double dSusceptible = _dSusceptible;
                double dExposed = _dExposed;
                double dInfectious =_dInfectious;

                _dSusceptible -= dΔSusceptibleToExposed(dSusceptible, dInfectious, this.Vaccinated, _iPopulation, this.InfectiousPeriod, this.Reproduction, this.Effectiveness);
                _dExposed += dΔSusceptibleToExposed(dSusceptible, dInfectious, this.Vaccinated, _iPopulation, this.InfectiousPeriod, this.Reproduction, this.Effectiveness) - dΔExposedToInfectious(dExposed, this.IncubationPeriod);
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
            sb.AppendFormat("Vaccinated:\t{0}\n", this.Vaccinated);
            return sb.ToString();
        }

        #endregion
    }
}

