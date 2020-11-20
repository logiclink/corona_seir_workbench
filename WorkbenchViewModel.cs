using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace LogicLink.Corona {

    /// <summary>
    /// ViewModel of the workbench with all workbench settings
    /// </summary>
    public class WorkbenchViewModel : INotifyPropertyChanged {

        #region Private Variables

        private string _sCountry;               // Country which should be calculated

        private int _iPopulation;               // Total number of individuals
        private int _iInfectious;               // Number of individuals which are infectious at the beginning
        private TimeSpan _tsIncubationPeriod;   // Timespan in which an individual is infected but not infectious
        private TimeSpan _tsInfectiousPeriod;   // Timespan in which an individual is infectious.
        private double _dReproduction;          // Basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune. transl. "Basisreproduktionszahl"

        private DateTime _dtStart;              // Start date of the calculation (27.1.2020, https://www.spiegel.de/wissenschaft/medizin/erster-corona-fall-in-deutschland-die-unglueckliche-reise-von-patientin-0-a-2096d364-dcd8-4ec8-98ca-7a8ca1d63524)
        private DateTime _dtEnd;                // End date of the calculation

        private bool _bShowSusceptible;         // If true, the (S)usceptible-series of the SEIR-object is shown
        private bool _bShowExposed;             // If true, the (E)xposed-series of the SEIR-object is shown
        private bool _bShowInfectious;          // If true, the (I)nfectious-series of the SEIR-object is shown
        private bool _bShowRemoved;             // If true, the (R)emoved-series of the SEIR-object is shown
        private bool _bShowCases;               // If true, the cases-series of the SEIR-object is shown
        private bool _bShowDaily;               // If true, daily cases-series of the SEIR-object is shown
        private bool _bShow7Days;               // If true, 7 day average of daily cases per 100.000-series of the SEIR-object is show
        private bool _bShowReproduction;        // If true, R₀-series of the SEIR-object is shown for the second axis
        private bool _bShowConfirmed;           // If true, confirmed-series of the JHU-data is shown
        private bool _bShowDailyConfirmed;      // If true, daily confirmed cases-series of the JHU-data is shown
        private bool _bShow7DaysConfirmed;      // If true, 7 day average of daily confirmed cases per 100.000-series of the JHU-data is shown
        private bool _bShowRecovered;           // If true, total recovered-series of the JHU-data is shown
        private bool _bShowDeaths;              // If true, total death-series of the JHU-data is shown
        private bool _bShowNowcasting;          // If true, R₀-series of the RKI-data is shown for the second axis
        private bool _bShowNowcasting7Day;      // If true, 7 days average R₀-series of the RKI-data is shown for the second axis
        private bool _bShowDoubledMarker;       // If true, double value diamond markers are added to cases-, daily- and 7days-series of the SEIR-object

        private bool _bSolveR0;                 // If true, R₀ is calculated for each day with JHU-data
        private bool _bSolveR0Interval;         // If true, R₀ is calculated for an intervall of days
        private int _iSolveR0ResidualDayWindow; // Number of days from 1 to n for residuals with which R₀ should be calculated.
        private int _iSolveR0IntervalDays;      // Number of days from 1 to n for the interval in which R₀ should be calculated.

        #endregion

        #region Public Properties

        /// <summary>
        /// Gest or sets the country which should be calculated
        /// </summary>
        public string Country {
            get { return _sCountry; }
            set { if(_sCountry != value) {
                    _sCountry = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the total number of individuals
        /// </summary>
        public int Population {
            get { return _iPopulation; }
            set { if(_iPopulation != value) {
                    _iPopulation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of individuals which are infectious at the beginning
        /// </summary>
        public int Infectious {
            get { return _iInfectious; }
            set { if(_iInfectious != value) {
                    _iInfectious = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the timespan in which an individual is infected but not infectious
        /// </summary>
        public TimeSpan IncubationPeriod {
            get { return _tsIncubationPeriod; }
            set { if(_tsIncubationPeriod != value) {
                    _tsIncubationPeriod = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the timespan in which an individual is infectious.
        /// </summary>
        public TimeSpan InfectiousPeriod {
            get { return _tsInfectiousPeriod; }
            set { if(_tsInfectiousPeriod != value) {
                    _tsInfectiousPeriod = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the basic reproduction number (R₀), number of individuals which are infected by an infectious individual if none of the population is immune. transl. "Basisreproduktionszahl"
        /// </summary>
        public double Reproduction {
            get { return _dReproduction; }
            set { if(_dReproduction != value) {
                    _dReproduction = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the start date of the calculation
        /// </summary>
        public DateTime Start {
            get { return _dtStart; }
            set { if(_dtStart != value) {
                    _dtStart = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the end date of the calculation
        /// </summary>
        public DateTime End {
            get { return _dtEnd; }
            set { if(_dtEnd != value) {
                    _dtEnd = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Public Show Properties

        /// <summary>
        /// Show 'Susceptible' series
        /// </summary>
        public bool ShowSusceptible {
            get { return _bShowSusceptible; }
            set { if(_bShowSusceptible != value) {
                    _bShowSusceptible = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Exposed' series
        /// </summary>
        public bool ShowExposed {
            get { return _bShowExposed; }
            set { if(_bShowExposed != value) {
                    _bShowExposed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Infectious' series
        /// </summary>
        public bool ShowInfectious {
            get { return _bShowInfectious; }
            set { if(_bShowInfectious != value) {
                    _bShowInfectious = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Removed' series
        /// </summary>
        public bool ShowRemoved {
            get { return _bShowRemoved; }
            set { if(_bShowRemoved != value) {
                    _bShowRemoved = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Cases' series
        /// </summary>
        public bool ShowCases {
            get { return _bShowCases; }
            set { if(_bShowCases != value) {
                    _bShowCases = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Daily' series
        /// </summary>
        public bool ShowDaily {
            get { return _bShowDaily; }
            set { if(_bShowDaily != value) {
                    _bShowDaily = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show '7 Days Incidence' series
        /// </summary>
        public bool Show7Days {
            get { return _bShow7Days; }
            set { if(_bShow7Days != value) {
                    _bShow7Days = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show '7 Days Confirmed Incidence' series
        /// </summary>
        public bool Show7DaysConfirmed {
            get { return _bShow7DaysConfirmed; }
            set { if(_bShow7DaysConfirmed != value) {
                    _bShow7DaysConfirmed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Reproduction' series
        /// </summary>
        public bool ShowReproduction {
            get { return _bShowReproduction; }
            set { if(_bShowReproduction != value) {
                    _bShowReproduction = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// Shows Marker in Cases, Daily & 7-Days series when values have doubled from today on
        /// </summary>
        public bool ShowDoubledMarker {
            get { return _bShowDoubledMarker; }
            set { if(_bShowDoubledMarker != value) {
                    _bShowDoubledMarker = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Confirmed' series
        /// </summary>
        public bool ShowConfirmed {
            get { return _bShowConfirmed; }
            set { if(_bShowConfirmed != value) {
                    _bShowConfirmed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Daily Confirmed' series
        /// </summary>
        public bool ShowDailyConfirmed {
            get { return _bShowDailyConfirmed; }
            set { if(_bShowDailyConfirmed != value) {
                    _bShowDailyConfirmed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Recovered' series
        /// </summary>
        public bool ShowRecovered {
            get { return _bShowRecovered; }
            set { if(_bShowRecovered != value) {
                    _bShowRecovered = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Show 'Deaths' series
        /// </summary>
        public bool ShowDeaths {
            get { return _bShowDeaths; }
            set { if(_bShowDeaths != value) {
                    _bShowDeaths = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Shows the RKI nowcasting R₀ value 
        /// </summary>
        public bool ShowNowcasting {
            get { return _bShowNowcasting; }
            set { if(_bShowNowcasting != value) {
                    _bShowNowcasting = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Shows the RKI nowcasting 7-Day-R₀ value
        /// </summary>
        public bool ShowNowcasting7Day {
            get { return _bShowNowcasting7Day; }
            set { if(_bShowNowcasting7Day != value) {
                    _bShowNowcasting7Day = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Public Solve Properties

        /// <summary>
        /// Solve R₀ if true
        /// </summary>
        public bool SolveR0 {
            get { return _bSolveR0; }
            set { if(_bSolveR0 != value) {
                    _bSolveR0 = value;
                    OnPropertyChanged();

                    if(value)
                        this.SolveR0Interval = false;
                  }
            }
        }

        /// <summary>
        /// Solve R₀ for an interval if true
        /// </summary>
        public bool SolveR0Interval {
            get { return _bSolveR0Interval; }
            set { if(_bSolveR0Interval != value) {
                    _bSolveR0Interval = value;
                    OnPropertyChanged();

                    if(value)
                        this.SolveR0 = false;
                  }
            }
        }

        /// <summary>
        /// Number of days from 1 to n for residuals with which R₀ should be calculated.
        /// </summary>
        public int SolveR0ResidualDayWindow {
            get { return _iSolveR0ResidualDayWindow; }
            set { if(_iSolveR0ResidualDayWindow != value) {
                    _iSolveR0ResidualDayWindow = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of days from 1 to n for the interval in which R₀ should be calculated.
        /// </summary>
        public int SolveR0IntervalDays {
            get { return _iSolveR0IntervalDays; }
            set { if(_iSolveR0IntervalDays != value) {
                    _iSolveR0IntervalDays = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads view model from settigs
        /// </summary>
        public void Load(bool bShow = true) {
            this.Country = Settings.Default.Country;
            this.Population = Settings.Default.Population;
            this.Infectious = Settings.Default.Infectious;
            this.IncubationPeriod = Settings.Default.IncubationPeriod;
            this.InfectiousPeriod = Settings.Default.InfectiousPeriod;
            this.Reproduction = Settings.Default.Reproduction;
            this.Start = Settings.Default.Start;
            this.End = Settings.Default.End;
            this.ShowSusceptible =    bShow && (Settings.Default.Show >>  0 & 1) == 1;
            this.ShowExposed =        bShow && (Settings.Default.Show >>  1 & 1) == 1;
            this.ShowInfectious =     bShow && (Settings.Default.Show >>  2 & 1) == 1;
            this.ShowRemoved =        bShow && (Settings.Default.Show >>  3 & 1) == 1;
            this.ShowCases =          bShow && (Settings.Default.Show >>  4 & 1) == 1;
            this.ShowDaily =          bShow && (Settings.Default.Show >>  5 & 1) == 1;
            this.Show7Days =          bShow && (Settings.Default.Show >>  6 & 1) == 1;
            this.ShowReproduction =   bShow && (Settings.Default.Show >>  7 & 1) == 1;
            this.ShowDoubledMarker =  bShow && (Settings.Default.Show >>  8 & 1) == 1;
            this.ShowConfirmed =      bShow && (Settings.Default.Show >>  9 & 1) == 1;
            this.ShowDailyConfirmed = bShow && (Settings.Default.Show >> 10 & 1) == 1;
            this.Show7DaysConfirmed = bShow && (Settings.Default.Show >> 12 & 1) == 1;
            this.ShowRecovered =      bShow && (Settings.Default.Show >> 13 & 1) == 1;
            this.ShowDeaths =         bShow && (Settings.Default.Show >> 14 & 1) == 1;
            this.ShowNowcasting =     bShow && (Settings.Default.Show >> 15 & 1) == 1;
            this.ShowNowcasting7Day = bShow && (Settings.Default.Show >> 16 & 1) == 1;
            this.SolveR0 =            (Settings.Default.Solve >> 0 & 1) == 1;
            this.SolveR0Interval =    (Settings.Default.Solve >> 1 & 1) == 1;
            this.SolveR0ResidualDayWindow = Settings.Default.SolveR0ResidualDayWindow;
            this.SolveR0IntervalDays = Settings.Default.SolveR0IntervalDays;
        }

        /// <summary>
        /// Saves view model to settings
        /// </summary>
        public void Save() {
            Settings.Default.Country = _sCountry;
            Settings.Default.Population = _iPopulation;
            Settings.Default.Infectious = _iInfectious;
            Settings.Default.IncubationPeriod = _tsIncubationPeriod;
            Settings.Default.InfectiousPeriod = _tsInfectiousPeriod;
            Settings.Default.Reproduction = _dReproduction;
            Settings.Default.Start = _dtStart;
            Settings.Default.End = _dtEnd;
            Settings.Default.Show = (_bShowSusceptible ?    1 <<  0 : 0) |
                                    (_bShowExposed ?        1 <<  1 : 0) |
                                    (_bShowInfectious ?     1 <<  2 : 0) |
                                    (_bShowRemoved ?        1 <<  3 : 0) |
                                    (_bShowCases ?          1 <<  4 : 0) |
                                    (_bShowDaily ?          1 <<  5 : 0) |
                                    (_bShow7Days ?          1 <<  6 : 0) |
                                    (_bShowReproduction ?   1 <<  7 : 0) |
                                    (_bShowDoubledMarker ?  1 <<  8 : 0) |
                                    (_bShowConfirmed ?      1 <<  9 : 0) |
                                    (_bShowDailyConfirmed ? 1 << 10 : 0) |
                                    (_bShow7DaysConfirmed ? 1 << 12 : 0) |
                                    (_bShowRecovered ?      1 << 13 : 0) |
                                    (_bShowDeaths ?         1 << 14 : 0) |
                                    (_bShowNowcasting ?     1 << 15 : 0) |
                                    (_bShowNowcasting7Day ? 1 << 16 : 0);
            Settings.Default.Solve = (_bSolveR0 ?           1 << 0 : 0) |
                                     (_bSolveR0Interval ?   1 << 1 : 0);
            Settings.Default.SolveR0ResidualDayWindow = _iSolveR0ResidualDayWindow;
            Settings.Default.SolveR0IntervalDays = _iSolveR0IntervalDays;
            Settings.Default.Save();
        }

        /// <summary>
        /// Ressets all changes to view model
        /// </summary>
        public void Reset() {
            Settings.Default.Reset();
            Load();
        }

        /// <summary>
        /// Clears the view model and shows no series
        /// </summary>
        public void Clear() {
            Settings.Default.Reset();
            Load(false);
        }

        #endregion

        #region INotifyPropertyChanged
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Feuert einen PropertyChanged-Event mit dem übergebenen Eigenschaftsnamen.
        /// </summary>
        /// <param name="propertyName">Name der Eigenschaft, die sich geändert hat</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

    }
}
