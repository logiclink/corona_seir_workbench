using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Threading;

using Microsoft.Win32;

namespace LogicLink.Corona {

    // TODO: MM210605 1. Impfzahlen für Wales, etc NICHT zu GB hinzuaddieren
    //                2. Fehlende Infektionszahlen fortschreiben (siehe Frankreich)

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Workbench : Window {

        /*
        private const int POPULATION_DE = 83019213;
        private const int INFECTIOUS_DE = 1;
        private readonly TimeSpan INCUBATION_PERIOD_DE = TimeSpan.FromDays(5.2);
        private readonly TimeSpan INFECTIOUS_PERIOD_DE = TimeSpan.FromDays(2.9);
        private const double REPRODUCTIONRATE_DE = 3.4d;

        // https://www.spiegel.de/wissenschaft/medizin/erster-corona-fall-in-deutschland-die-unglueckliche-reise-von-patientin-0-a-2096d364-dcd8-4ec8-98ca-7a8ca1d63524
        private readonly DateTime FIRST_CASE = new DateTime(2020, 1, 27); 
        private readonly DateTime REPRODUCTION_START = new DateTime(2020, 3, 6); 
        */

        private Progress _pgr = new Progress();
        private System.Timers.Timer _tmr = new System.Timers.Timer(250);
        private bool _bUpdateReproduction = false;                          // True, if basic reproduction numbers (R₀) should be re-calculated
        private bool _bUpdateVaccination = false;                           // True, if vaccinations should be re-calculated
        private bool _bUpdateDailyVaccination = true;                       // True, if the number of daily vaccinated people should be re-calculated from the last 7 days
        private bool _bManualVaccination = false;                           // True, if OWID vaccination data should be ignored

        private Dictionary<DateTime, double> _dicReproduction;              // Dictionary of basic reproduction numbers (R₀) 
        private Dictionary<DateTime, double> _dicVaccinated;                // Dictionary of vaccinates individuals per day

        private bool _bUpdating = false;

        private async Task UpdateReproductionAsync(WorkbenchViewModel vm) {
            _bUpdateReproduction = false;
            bool bResetUpdating = !_bUpdating;
            _bUpdating = true;

            if(vm.SolveR0 || vm.SolveR0Interval) {
                try {
                    _pgr.Report(1, "Calculating basic reproduction numbers ...", true);

                    List<JHU.Record> lConfirmedRecords = await new JHU().GetDataAsync(vm.Country).ToListAsync();
                    _pgr.Report(2);

                    DateTime dtStart = DateTimeHelper.Max(lConfirmedRecords[0].Date, vm.Start);
                    DateTime dtEnd = DateTimeHelper.Min(lConfirmedRecords[^1].Date, vm.End);

                    List<int> lConfirmed = lConfirmedRecords.Skip((dtStart - lConfirmedRecords[0].Date).Days).Take((dtEnd - dtStart).Days + 1).Select(r => r.Confirmed).ToList();
                    _pgr.Report(3);

                    List<OWID.Record> lVaccinatedRecords = await new OWID().GetDataAsync(vm.Country).ToListAsync();
                    _pgr.Report(4);

                    if(lVaccinatedRecords.Any()) {
                        SEIRV seirvR0 = new SEIRV(vm.Population, vm.Infectious, vm.IncubationPeriod, vm.InfectiousPeriod, vm.Reproduction, vm.Effectiveness, vm.ProtectionStartPeriod);
                        _pgr.Report(5);

                        // Align model to confirmed numbers
                        // REMARKS: There can't be any vaccination data because it's aligned to confirmed numbers
                        for(int i = 1; i <= (dtStart - vm.Start).Days; i++)
                            seirvR0.Calc(i);
                        _pgr.Report(6);

                        // Align vaccinated individuals with confirmed cases
                        // REMARKS: For future vaccinations the average of the daily vaccinations in the last 5 days is calculated
                        List<double> lVaccinated = OWID.Record.AlignVaccinated(lVaccinatedRecords,
                                                                               dtStart, 0,
                                                                               dtEnd, (int)Math.Round(lVaccinatedRecords.TakeLast(7).Select(r => r.DailyVaccinated).Average())).ToList();
                        _pgr.Report(7);

                        Progress pgrR0 = new Progress(8, 90);
                        pgrR0.Changed += _pgr_Changed;

                        // Create dictionary of R₀ values per date
                        _dicReproduction = new Dictionary<DateTime, double>();
                        int j = 0;
                        IR0Solver slr = vm.SolveR0 
                                        ? new SEIRVR0Solver(vm.SolveR0ResidualDayWindow) { SEIRV = seirvR0, Confirmed = lConfirmed, Vaccinated = lVaccinated } as IR0Solver
                                        : new SEIRVR0IntervalSolver(vm.SolveR0IntervalDays) { SEIRV = seirvR0, Confirmed = lConfirmed, Vaccinated = lVaccinated };
                        foreach(double d in slr.Solve(pgrR0))
                            _dicReproduction.Add(dtStart.AddDays(j++), d);

                        pgrR0.Changed -= _pgr_Changed;

                    } else {

                        SEIR seirR0 = new SEIR(vm.Population, vm.Infectious, vm.IncubationPeriod, vm.InfectiousPeriod, vm.Reproduction);
                        _pgr.Report(5);

                        // Align model to confirmed numbers
                        for(int i = 1; i <= (dtStart - vm.Start).Days; i++)
                            seirR0.Calc(i);
                        _pgr.Report(6);

                        Progress pgrR0 = new Progress(7, 91);
                        pgrR0.Changed += _pgr_Changed;

                        // Create dictionary of R₀ values per date
                        _dicReproduction = new Dictionary<DateTime, double>();
                        int j = 0;
                        IR0Solver slr = vm.SolveR0
                                        ? new SEIRR0Solver(vm.SolveR0ResidualDayWindow) { SEIR = seirR0, Confirmed = lConfirmed } as IR0Solver
                                        : new SEIRR0IntervalSolver(vm.SolveR0IntervalDays) { SEIR = seirR0, Confirmed = lConfirmed };
                        foreach(double d in slr.Solve(pgrR0))
                            _dicReproduction.Add(dtStart.AddDays(j++), d);

                        pgrR0.Changed -= _pgr_Changed;
                    }

                    _pgr.Report(99);

                    // Calculate R₀ average of last 5 days
                    vm.Reproduction = _dicReproduction.Values.TakeLast(5).Average();

                    _pgr.Report(100);

                } catch(Exception ex) {
                    MessageBox.Show($"Basic reproduction numbers calculation error\n\n{ex.GetMostInnerException().Message}", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            } else
                _dicReproduction = null;

            if(bResetUpdating)
                _bUpdating = false;
        }

        private async Task UpdateVaccinationAsync(WorkbenchViewModel vm) {
            _bUpdateVaccination = false;
            bool bResetUpdating = !_bUpdating;
            _bUpdating = true;

            try {
                _pgr.Report(1, "Calculating vaccination numbers ...", true);

                List<OWID.Record> lVaccinatedRecords = null;
                if(!_bManualVaccination)
                    lVaccinatedRecords = await new OWID().GetDataAsync(vm.Country).ToListAsync();
                _dicVaccinated = new Dictionary<DateTime, double>();
                _pgr.Report(30);

                if(lVaccinatedRecords?.Count > 0 && (_bUpdateDailyVaccination || vm.VaccinationStart == lVaccinatedRecords[0].Date)) {    // Calculate vaccination parameter from OWID vaccinated values
                    if(_bUpdateDailyVaccination) {
                        vm.DailyVaccinated = (int)Math.Round(lVaccinatedRecords.TakeLast(7).Select(r => r.DailyVaccinated).Average());
                        vm.VaccinationStart = lVaccinatedRecords[0].Date;
                        _bUpdateDailyVaccination = false;
                    }

                    // Align vaccinated individuals with confirmed cases
                    // REMARKS: For future vaccinations the average of the daily vaccinations in the last 5 days is calculated
                    DateTime dt = vm.VaccinationStart;
                    foreach(double dVaccinated in OWID.Record.AlignVaccinated(lVaccinatedRecords,
                                                                              vm.VaccinationStart, 0,
                                                                              vm.End, vm.DailyVaccinated)) {
                        _dicVaccinated.Add(dt, dVaccinated);
                        dt = dt.AddDays(1);
                        _pgr.Report(30 + (dt - vm.VaccinationStart).Days / (vm.End - vm.VaccinationStart).Days * 69);
                    }
                } else if(vm.VaccinationStart <= vm.End && vm.DailyVaccinated > 0) {                        // Calculate vaccination parameter from manual values
                    int iVaccinated = 0;
                    for(DateTime dt = vm.VaccinationStart; dt <= vm.End; dt = dt.AddDays(1)) {
                        _dicVaccinated.Add(dt, iVaccinated += vm.DailyVaccinated);
                        _pgr.Report(30 + (dt - vm.VaccinationStart).Days / (vm.End - vm.VaccinationStart).Days * 69);
                    }
                }
                _pgr.Report(100);
            } catch(Exception ex) {
                MessageBox.Show($"Vaccination numbers calculation error\n\n{ex.GetMostInnerException().Message}", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            if(bResetUpdating)
                _bUpdating = false;
        }

        private void InitChart() {

            // Font for axis titles & legend
            Font fntTitle = new Font(cht.ChartAreas[0].AxisY.LabelStyle.Font.FontFamily, cht.ChartAreas[0].AxisY.LabelStyle.Font.Size * 2f, System.Drawing.FontStyle.Bold);
            Font fntAxis = new Font(cht.ChartAreas[0].AxisY.LabelStyle.Font.FontFamily, cht.ChartAreas[0].AxisY.LabelStyle.Font.Size * 1.25f);

            // Title
            Title t = cht.Titles.Add(string.Empty);
            t.Font = fntTitle;
            t.DockedToChartArea = cht.ChartAreas[0].Name;//  "Default";
            t.IsDockedInsideChartArea = true;

            // X-Axis settings
            cht.ChartAreas[0].AxisX.TitleFont = fntAxis;

            // Y-Axis settings
            cht.ChartAreas[0].AxisY.TitleFont = fntAxis;
            cht.ChartAreas[0].AxisY.Title = "Individuals";
            cht.ChartAreas[0].AxisY.LabelStyle.Format = "N0";
            cht.ChartAreas[0].AxisY.Minimum = 0d;
            cht.ChartAreas[0].AxisY.MinorTickMark.Enabled = true;
            cht.ChartAreas[0].AxisY.MinorTickMark.LineColor = Color.LightGray;

            cht.ChartAreas[0].AxisY2.TitleFont = fntAxis;
            cht.ChartAreas[0].AxisY2.Title = "R₀";
            cht.ChartAreas[0].AxisY2.Minimum = 0d;
            cht.ChartAreas[0].AxisY2.Maximum = 10d;
            cht.ChartAreas[0].AxisY2.Interval = 1d;
            cht.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            cht.ChartAreas[0].AxisY2.MinorTickMark.Enabled = true;
            cht.ChartAreas[0].AxisY2.MinorTickMark.Interval = .1d;
            cht.ChartAreas[0].AxisY2.MinorTickMark.LineColor = Color.LightGray;

        }

        private async Task UpdateChartAsync(WorkbenchViewModel vm) {
            bool bResetUpdating = !_bUpdating;
            _bUpdating = true;

            try {
                _pgr.Report(1, "Calculating chart series ...", true);

                // 1. Calculate SEIR model and create series
                ISEIR seir = new SEIRV(vm.Population - vm.Infectious, vm.Infectious, vm.IncubationPeriod, vm.InfectiousPeriod, vm.Reproduction, vm.Effectiveness, vm.ProtectionStartPeriod);
                IDateSeriesView vSeir = _dicReproduction == null
                                        ? _dicVaccinated == null 
                                          ? new SEIRDateSeriesView(seir, vm.ShowSusceptible, vm.ShowExposed, vm.ShowInfectious, vm.ShowRemoved, vm.ShowCases, vm.ShowDaily, vm.Show7Days, vm.ShowReproduction, vm.ShowDoubledMarker)
                                          : new SEIRVDateSeriesView((ISEIRV)seir, _dicVaccinated, vm.ShowSusceptible, vm.ShowExposed, vm.ShowInfectious, vm.ShowRemoved, vm.ShowCases, vm.ShowDaily, vm.Show7Days, vm.ShowReproduction, vm.ShowDoubledMarker, vm.ShowVaccinated, vm.ShowDailyVaccinated)
                                        : _dicVaccinated == null 
                                          ? new SEIRR0DateSeriesView(seir, _dicReproduction, vm.ShowSusceptible, vm.ShowExposed, vm.ShowInfectious, vm.ShowRemoved, vm.ShowCases, vm.ShowDaily, vm.Show7Days, vm.ShowReproduction, vm.ShowDoubledMarker)
                                          : new SEIRVR0DateSeriesView((ISEIRV)seir, _dicReproduction, _dicVaccinated, vm.ShowSusceptible, vm.ShowExposed, vm.ShowInfectious, vm.ShowRemoved, vm.ShowCases, vm.ShowDaily, vm.Show7Days, vm.ShowReproduction, vm.ShowDoubledMarker, vm.ShowVaccinated, vm.ShowDailyVaccinated);
                if(vm.ShowSusceptible || vm.ShowExposed || vm.ShowInfectious || vm.ShowRemoved || vm.ShowVaccinated || vm.ShowDailyVaccinated || vm.ShowCases || vm.ShowDaily || vm.Show7Days || vm.ShowReproduction) {
                    Progress pgrSeir = new Progress(2, 31);
                    pgrSeir.Changed += _pgr_Changed;
                    await vSeir.CalcAsync(vm.Start, vm.End, pgrSeir);
                    pgrSeir.Changed -= _pgr_Changed;
                }

                // 2. Get JHU data and create series
                JHU jhu = new JHU();
                JHUDateSeriesView vJhu = new JHUDateSeriesView(jhu, vm.Country, vm.Population, vm.ShowConfirmed, vm.ShowDailyConfirmed, vm.Show7DaysConfirmed, vm.ShowRecovered, vm.ShowDeaths);
                if(vm.ShowConfirmed || vm.ShowDailyConfirmed  || vm.Show7DaysConfirmed || vm.ShowRecovered || vm.ShowDeaths) {
                    Progress pgrJhu = new Progress(34, 19);
                    pgrJhu.Changed += _pgr_Changed;
                    try {
                        await vJhu.CalcAsync(vm.Start, vm.End, pgrJhu);
                    } catch(Exception ex) {
                        MessageBox.Show($"Chart updating error because of an error in the JHU data file\n\n{ex.GetMostInnerException().Message}\n\nPlease check the file format.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        vm.ShowConfirmed = false;
                        vm.ShowDailyConfirmed = false;
                        vm.Show7DaysConfirmed = false;
                        vm.ShowRecovered = false;
                        vm.ShowDeaths = false;
                    }
                    pgrJhu.Changed -= _pgr_Changed;
                }

                // 3. Get OWID data and create series
                OWID owid = new OWID();
                OWIDDateSeriesView vOwid = new OWIDDateSeriesView(owid, vm.Country, vm.ShowConfirmedVaccinated, vm.ShowDailyConfirmedVaccinated);
                if(vm.ShowConfirmedVaccinated || vm.ShowDailyConfirmedVaccinated) {
                    Progress pgrOwd = new Progress(54, 10);
                    pgrOwd.Changed += _pgr_Changed;
                    try {
                        await vOwid.CalcAsync(vm.Start, vm.End, pgrOwd);
                    } catch(Exception ex) {
                        MessageBox.Show($"Chart updating error because of an error in the OWID data file\n\n{ex.GetMostInnerException().Message}\n\nPlease check the file format.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        vm.ShowConfirmedVaccinated = false;
                        vm.ShowDailyConfirmedVaccinated = false;
                    }
                    pgrOwd.Changed -= _pgr_Changed;
                }

                // 4. Get RKI nowcasting data and create series
                RKINowcasting rnc = new RKINowcasting();
                RKINowcastingDateSeriesView vRnc = new RKINowcastingDateSeriesView(rnc, vm.ShowNowcasting7Day);
                if(vm.ShowNowcasting7Day) {
                    Progress pgrRnc = new Progress(65, 29);
                    pgrRnc.Changed += _pgr_Changed;
                    try {
                        await vRnc.CalcAsync(vm.Start, vm.End, pgrRnc);
                    } catch(Exception ex) {
                        MessageBox.Show($"Chart updating error because of an error in the RKI Nowcasting data file\n\n{ex.GetMostInnerException().Message}\n\nPlease check the file format.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        vm.ShowNowcasting7Day = false;
                    }
                    pgrRnc.Changed -= _pgr_Changed;
                }

                // 5. Update Title
                cht.Titles[0].Text = vm.Country;

                // 6. Add strip lines for weekend
                cht.ChartAreas[0].AxisX.StripLines.Clear();
                cht.ChartAreas[0].AxisX.StripLines.Add(new StripLine { IntervalOffset = -0.5d - (int)vm.Start.DayOfWeek,
                                                                       Interval = 7,
                                                                       StripWidth = 2,
                                                                       BackColor = Color.FromArgb(247, 247, 247) });

                // 7. Show available series
                cht.Series.Clear();
                _pgr.Report(95);
                cht.Series.Add(vSeir);
                _pgr.Report(96);
                cht.Series.Add(vJhu);
                _pgr.Report(97);
                cht.Series.Add(vOwid);
                _pgr.Report(98);
                cht.Series.Add(vRnc);
                _pgr.Report(99);

                cht.ChartAreas[0].RecalculateAxesScale();

                _pgr.Report(100);

                // 8. Add Legend
                cht.Legends.Clear();
                cht.Legends.Add(new Legend("Corona SEIR") { Font = cht.ChartAreas[0].AxisX.TitleFont });
            
                _pgr.Report(100, "Ready", false);
            } catch(Exception ex) {
                MessageBox.Show($"Chart updating error\n\n{ex.GetMostInnerException().Message}", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            if(bResetUpdating)
                _bUpdating = false;
        }

        /// <summary>
        /// Updates the population number from World Bank data
        /// </summary>
        /// <param name="vm">View model</param>
        /// <returns>Awaitable Task</returns>
        /// <remarks>
        /// Use data from previous year as data for the current year might not exist.
        /// </remarks> 
        private async Task UpdatePopulationAsync(WorkbenchViewModel vm) {
            try {
                vm.Population = await new WPPopulation().GetDataAsync(vm.Country, vm.Start.Year);
            } catch(Exception ex) {
                MessageBox.Show($"World Bank population data loading error\n\n{ex.GetMostInnerException().Message}", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        /// <summary>
        /// Updates the initial number infectious individuals from Johns Hopkins University data. Minimum is 1.
        /// </summary>
        /// <param name="vm">View model</param>
        /// <returns>Awaitable Task</returns>
        private async Task UpdateInfectiousAsync(WorkbenchViewModel vm) {
            await foreach(JHU.Record r in new JHU().GetDataAsync(vm.Country)) {
                if(r.Date > vm.Start) break;
                if(r.Date == vm.Start) {
                    vm.Infectious = Math.Max(r.Confirmed, 1);
                    break;
                }
            }
        }

        #region Constructors & Destructors

        public Workbench() {
            InitializeComponent();
            this.Title = $"{this.Title} {Assembly.GetExecutingAssembly().GetName().Version}";

            _pgr.Changed += _pgr_Changed;
            _tmr.Elapsed += _tmr_Elapsed;
            cht.PostPaint += cht_PostPaint;
            cht.GetToolTipText += cht_GetToolTipText;
        }

        #endregion

        #region Window Events

        private void Workbench_Loaded(object sender, RoutedEventArgs e) {
            if(!(this.DataContext is WorkbenchViewModel vm)) return;

            // Initialize view model
            vm.Load();                                  // Load Settings
            vm.PropertyChanged += vm_PropertyChanged;

            // Initialize Johns Hopkins University data
            Dispatcher.BeginInvoke((Action)(async () => {
                try {
                    await new JHU().LoadAsync();
                } catch(Exception ex) {
                    MessageBox.Show($"Johns-Hopkins-University CSSE loading error\n\n{ex.GetMostInnerException().Message}", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
                try {
                    await new OWID().LoadAsync();
                } catch(Exception ex) {
                    MessageBox.Show($"Our World In Data loading error\n\n{ex.GetMostInnerException().Message}", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
        }), DispatcherPriority.Background);

            InitChart();
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => { if(vm.SolveR0 || vm.SolveR0Interval)
                                                                                            await UpdateReproductionAsync(vm);
                                                                                         await UpdateVaccinationAsync(vm);
                                                                                         await UpdateChartAsync(vm); 
                                                                                        }));
        }

        #endregion

        #region Control Events

        private void _pgr_Changed(object sender, ValueEventArgs<(int Progress, string Message, bool Show)> e) {
            pbr.Value = e.Value.Progress;

            if(e.Value.Message != default)
                sbi.Content = e.Value.Message;

            if(e.Value.Show && pbr.Visibility != Visibility.Visible)
                pbr.Visibility = Visibility.Visible;
            else if(!e.Value.Show && pbr.Visibility != Visibility.Collapsed)
                pbr.Visibility = Visibility.Collapsed;
            
            pbr.Dispatcher.Invoke(delegate () { }, DispatcherPriority.Render);
        }

        private void btnData_Click(object sender, RoutedEventArgs e) => new Data(cht.ToDataTable()).ShowDialog();

        private void btnExport_Click(object sender, RoutedEventArgs e) {
            if(!(this.DataContext is WorkbenchViewModel vm)) return;

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Title = "Export chart";
            sfd.FileName = vm.Country;                  // Default file name
            sfd.DefaultExt = ".png";                    // Default file extension
            sfd.Filter = "PNG (*.png)|*.png|Bitmap (*.bmp;*.dib)|*.bmp;*.dib|Enhanced Meta File (*.emf)|*.emf|GIF (*.gif)|*.gif|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF (*.tif;*.tiff)|*.tif;*.tiff|CSV (*.csv;*.txt)|*.csv;*.txt"; // Filter files by extension

            if(sfd.ShowDialog() == true)
                Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => {
                    string sExtension = Path.GetExtension(sfd.FileName).ToLower();
                    switch(sExtension) {
                        case ".csv":
                        case ".txt":
                            _pgr.Report(1, "Exporting chart as csv...", true);
                            Progress pgr = new Progress(0, 100);
                            pgr.Changed += _pgr_Changed;
                            await cht.ToCSVAsync(sfd.FileName, pgr);
                            pgr.Changed -= _pgr_Changed;
                            _pgr.Report(100, "Ready", false);
                            break;

                        default:
                            _pgr.Report(30, "Exporting chart as image...", true);
                            cht.SaveImage(sfd.FileName, sExtension switch {
                                ".bmp" => ChartImageFormat.Bmp,
                                ".emf" => ChartImageFormat.Emf,
                                ".gif" => ChartImageFormat.Gif,
                                ".jpg" => ChartImageFormat.Jpeg,
                                ".png" => ChartImageFormat.Png,
                                ".tif" => ChartImageFormat.Tiff,
                                _ => throw new NotSupportedException($"The file format '{Path.GetExtension(sfd.FileName)} is not supported.") });
                            _pgr.Report(100);
                            _pgr.Report(100, "Ready", false);
                            break;
                    }
                }));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            if(!(this.DataContext is WorkbenchViewModel vm)) return;

            vm.Save();
        }

        private async void btnReset_Click(object sender, RoutedEventArgs e) {
            if(!(this.DataContext is WorkbenchViewModel vm)) return;

            _bUpdateReproduction = true;
            _bUpdateVaccination = true;

            vm.Reset();

            await UpdatePopulationAsync(vm);
            await UpdateInfectiousAsync(vm);

            _bUpdateDailyVaccination = true;
            _bManualVaccination = false;
        }

        private async void btnClear_Click(object sender, RoutedEventArgs e) {
            if(!(this.DataContext is WorkbenchViewModel vm)) return;

            _bUpdateReproduction = true;
            _bUpdateVaccination = true;

            vm.Clear();

            await UpdatePopulationAsync(vm);
            await UpdateInfectiousAsync(vm);

            _bUpdateDailyVaccination = true;
            _bManualVaccination = false;
        }

        #endregion

        #region Chart Control Events

        private void cht_PostPaint(object sender, ChartPaintEventArgs e) {

            // HACK: Draw Legend with correct line widths
            //       see https://social.msdn.microsoft.com/Forums/vstudio/en-US/619ee184-7edc-4357-88d7-a168c72d8afc/line-chart-control-line-thickness-in-the-legend
            //       for idea. Currently, the title of the legend is not taken into account.
            if(e.ChartElement is Legend l) {
                Chart c = sender as Chart;
                Graphics g = e.ChartGraphics.Graphics;

                // Absolute dimensions of the legend (New legend will be based on this.. won't be exact.)
                RectangleF rctLegend = e.ChartGraphics.GetAbsoluteRectangle(l.Position.ToRectangleF());
                RectangleF rct = new RectangleF(rctLegend.X + 15f, rctLegend.Y + 5f, rctLegend.Width - 30f, rctLegend.Height - 10f); // Left, top, right & bottom margin 

                // Maximum text width and height
                float fTextWidth = 0f;
                float fTextHeightSum = 0f;
                foreach(string s in c.Series.Select(ser => ser.Name)) {
                    fTextWidth = Math.Max(fTextWidth, g.MeasureString(s, l.Font).Width);
                    fTextHeightSum += g.MeasureString(s, l.Font).Height;
                }

                // Size of one legend text "cell"
                SizeF szText = new SizeF(fTextWidth, fTextHeightSum / c.Series.Count);

                // Padding between line and text (horizontal) and each item (vertical)
                float fPadHorz = 10f;
                float fPadVert = 1f;

                // Draw a white box on top of the default legend to hide it
                g.FillRectangle(Brushes.White, rctLegend);

                // Brushes
                Brush brBack = new SolidBrush(l.BackColor);
                Brush brFront = new SolidBrush(l.ForeColor);

                // Draw a box with the BackColor of the legend
                g.FillRectangle(brBack, rctLegend);

                for(int i = 0; i < c.Series.Count; i++) {
                    Series s = c.Series[i];

                    // Remarks:  Line no thicker than the item height.
                    Pen p = new Pen(s.Color, Math.Min(s.BorderWidth, szText.Height)) { DashStyle = s.BorderDashStyle switch { ChartDashStyle.Dash => DashStyle.Dash,
                                                                                                                              ChartDashStyle.Dot => DashStyle.Dot,
                                                                                                                              _ => DashStyle.Solid
                                                                                                                            }};

                    float posY = rct.Y + fPadVert + (szText.Height + fPadVert ) * i + szText.Height / 2;
                    PointF ptStart = new PointF(rct.X, posY);
                    PointF ptEnd = new PointF(rct.Right - szText.Width - fPadHorz, posY);
                    switch(s.ChartType) {
                        case SeriesChartType.Column:
                        case SeriesChartType.RangeColumn:
                        case SeriesChartType.StackedColumn:
                        case SeriesChartType.StackedColumn100:
                        case SeriesChartType.Bar:
                        case SeriesChartType.ErrorBar:
                        case SeriesChartType.RangeBar:
                        case SeriesChartType.StackedBar:
                        case SeriesChartType.StackedBar100:
                            g.DrawRectangle(p, rct.X, rct.Y + fPadVert + fPadHorz / 2 + (szText.Height + fPadVert) * i, rct.Width - szText.Width - fPadHorz, szText.Height - fPadHorz);
                            break;
                        default:
                            g.DrawLine(p, ptStart, ptEnd);
                            break;
                    }

                    // Text
                    posY = rct.Y + fPadVert + (szText.Height + fPadVert) * i;
                    ptStart = new PointF(rct.Right - szText.Width, posY);
                    g.DrawString(s.Name, l.Font, brFront, ptStart);
                }
            }
        }

        private void cht_GetToolTipText(object sender, ToolTipEventArgs e) {
            e.Text = e.HitTestResult.ChartElementType switch {
                ChartElementType.DataPoint => e.HitTestResult.Series.YAxisType == AxisType.Primary 
                                              ? e.HitTestResult.Series.Points[e.HitTestResult.PointIndex].YValues[0].ToString(((Chart)sender).ChartAreas[0].AxisY.LabelStyle.Format) + " " + 
                                                e.HitTestResult.Series.Name + " on " +
                                                DateTime.FromOADate(e.HitTestResult.Series.Points[e.HitTestResult.PointIndex].XValue).ToString("d")

                                              : e.HitTestResult.Series.Points[e.HitTestResult.PointIndex].YValues[0].ToString(((Chart)sender).ChartAreas[0].AxisY2.LabelStyle.Format) + " " + 
                                                e.HitTestResult.Series.Name + " on " + 
                                                DateTime.FromOADate(e.HitTestResult.Series.Points[e.HitTestResult.PointIndex].XValue).ToString("d"),
                _ => e.Text
            };
        }

        #endregion

        #region Timer Events
        
        private void _tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            if(!_bUpdating) {
                _bUpdating = true;
                _tmr.Stop();
                Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => { if(_bUpdateReproduction)
                                                                                                await UpdateReproductionAsync(this.DataContext as WorkbenchViewModel);
                                                                                             if(_bUpdateVaccination)
                                                                                                await UpdateVaccinationAsync(this.DataContext as WorkbenchViewModel);
                                                                                             await UpdateChartAsync(this.DataContext as WorkbenchViewModel); 
                                                                                             _bUpdating = false;
                                                                                           }));
            }
        }

        #endregion

        #region ViewModel Events

        private void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(!(sender is WorkbenchViewModel vm)) return;

            switch(e.PropertyName) {
                case nameof(WorkbenchViewModel.Country):
                    _bUpdating = true;
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => { await UpdatePopulationAsync(vm); 
                                                                                                 await UpdateInfectiousAsync(vm);
                                                                                                 _bUpdating = false;
                                                                                                 _bUpdateReproduction = true;
                                                                                                 _bUpdateDailyVaccination = true;
                                                                                                 _bUpdateVaccination = true;
                                                                                                 _bManualVaccination = false;
                                                                                                 _tmr.Start();
                                                                                               }));
                    break;

                case nameof(WorkbenchViewModel.Start):
                    _bUpdating = true;
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => { await UpdateInfectiousAsync(vm);
                                                                                                 _bUpdating = false;
                                                                                                 _bUpdateReproduction = true;
                                                                                                 _tmr.Start();
                                                                                               }));
                    break;

                case nameof(WorkbenchViewModel.End):
                case nameof(WorkbenchViewModel.Population):
                case nameof(WorkbenchViewModel.Infectious):
                case nameof(WorkbenchViewModel.IncubationPeriod):
                case nameof(WorkbenchViewModel.InfectiousPeriod):
                case nameof(WorkbenchViewModel.SolveR0):
                case nameof(WorkbenchViewModel.SolveR0Interval):
                case nameof(WorkbenchViewModel.SolveR0ResidualDayWindow):
                case nameof(WorkbenchViewModel.SolveR0IntervalDays):
                case nameof(WorkbenchViewModel.Effectiveness):
                    if(!_bUpdating) {
                        _bUpdateReproduction = true;
                        _bUpdateVaccination = true;
                        _tmr.Start();
                    }
                    break;

                case nameof(WorkbenchViewModel.Reproduction):
                    if(!_bUpdating)
                        _tmr.Start();
                    break;

                case nameof(WorkbenchViewModel.DailyVaccinated):
                    if(!_bUpdating) {
                        _bUpdateVaccination = true;
                        _bManualVaccination = false;
                        _tmr.Start();
                    }
                    break;

                case nameof(WorkbenchViewModel.VaccinationStart):
                    if(!_bUpdating) {
                        _bUpdateVaccination = true;
                        _bManualVaccination = true;
                        _tmr.Start();
                    }
                    break;

                default:
                    _tmr.Start();
                    break;
            }
        }

        #endregion
    }
}
