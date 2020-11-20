using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Threading;

using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.Win32;

namespace LogicLink.Corona {

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
        private bool _bUpdateReproduction = false;

        private Dictionary<DateTime, double> _dicReproduction;              // Dictionary of basic reproduction numbers (R₀) 

        private bool _bUpdating = false;

        private async Task UpdateReproductionAsync(WorkbenchViewModel vm) {
            _bUpdateReproduction = false;
            _bUpdating = true;

            if(vm.SolveR0 || vm.SolveR0Interval) {
                _pgr.Report(1, "Calculating basic reproduction numbers ...", true);

                SEIR seirR0 = new SEIR(vm.Population, vm.Infectious, vm.IncubationPeriod, vm.InfectiousPeriod, vm.Reproduction);
                List<JHU.Record> l = await new JHU().GetDataAsync(vm.Country).ToListAsync();
                _pgr.Report(5);

                // Align confirmed numbers with model
                for(int i = 1; i <= (l[0].Date - vm.Start).Days; i++)
                    seirR0.Calc(i);
                List<int> ll = l.Skip((vm.Start - l[0].Date).Days).Select(r => r.Confirmed).ToList();
                _pgr.Report(6);

                Progress pgrR0 = new Progress(7, 91);
                pgrR0.Changed += _pgr_Changed;

                // Create dictionary of R₀ values per date
                _dicReproduction = new Dictionary<DateTime, double>();
                int j = 0;
                DateTime dt = l[0].Date < vm.Start ? vm.Start : l[0].Date;
                IR0Solver slr = vm.SolveR0 
                                ? new SEIRR0Solver(vm.SolveR0ResidualDayWindow) { SEIR = seirR0, Confirmed = ll } as IR0Solver
                                : new SEIRR0IntervalSolver(vm.SolveR0IntervalDays) { SEIR = seirR0, Confirmed = ll };
                foreach(double d in slr.Solve(pgrR0))
                    _dicReproduction.Add(dt.AddDays(j++), d);

                _pgr.Report(99);

                // Calculate R₀ median of last 5 days
                double dR0Sum = 0d;
                for(int k = j - 5; k < j; k++)
                    dR0Sum += _dicReproduction[dt.AddDays(k)];
                //for(DateTime dtFuture = dt.AddDays(j); dtFuture <= vm.End; dtFuture = dtFuture.AddDays(1))
                //    _dicReproduction.Add(dtFuture, dR0Sum / 5);
                vm.Reproduction = dR0Sum / 5;

                pgrR0.Changed -= _pgr_Changed;

                _pgr.Report(100);
            } else
                _dicReproduction = null;

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
            _bUpdating = true;

            _pgr.Report(1, "Calculating chart series ...", true);

            // 2. Calculate SEIR model and create series
            ISEIR seir = new SEIR(vm.Population - vm.Infectious, vm.Infectious, vm.IncubationPeriod, vm.InfectiousPeriod, vm.Reproduction);
            IDateSeriesView vSeir = _dicReproduction == null
                                    ? (IDateSeriesView) new SEIRDateSeriesView(seir, vm.ShowSusceptible, vm.ShowExposed, vm.ShowInfectious, vm.ShowRemoved, vm.ShowCases, vm.ShowDaily, vm.Show7Days, vm.ShowReproduction, vm.ShowDoubledMarker)
                                    : new SEIRR0DateSeriesView(seir, _dicReproduction, vm.ShowSusceptible, vm.ShowExposed, vm.ShowInfectious, vm.ShowRemoved, vm.ShowCases, vm.ShowDaily, vm.Show7Days, vm.ShowReproduction, vm.ShowDoubledMarker);
            if(vm.ShowSusceptible || vm.ShowExposed || vm.ShowInfectious || vm.ShowRemoved || vm.ShowCases || vm.ShowDaily || vm.Show7Days || vm.ShowReproduction) {
                Progress pgrSeir = new Progress(2, 31);
                pgrSeir.Changed += _pgr_Changed;
                await vSeir.CalcAsync(vm.Start, vm.End, pgrSeir);
                pgrSeir.Changed -= _pgr_Changed;
            }

            // 3. Get JHE data and create series
            JHU jhu = new JHU();
            JHUDateSeriesView vJhu = new JHUDateSeriesView(jhu, vm.Country, vm.Population, vm.ShowConfirmed, vm.ShowDailyConfirmed, vm.Show7DaysConfirmed, vm.ShowRecovered, vm.ShowDeaths);
            if(vm.ShowConfirmed || vm.ShowDailyConfirmed  || vm.Show7DaysConfirmed || vm.ShowRecovered || vm.ShowDeaths) {
                Progress pgrJhu = new Progress(34, 30);
                pgrJhu.Changed += _pgr_Changed;
                await vJhu.CalcAsync(vm.Start, vm.End, pgrJhu);
                pgrJhu.Changed -= _pgr_Changed;
            }

            // 3. Get RKI nowcasting data and create series
            RKINowcasting rnc = new RKINowcasting();
            RKINowcastingDateSeriesView vRnc = new RKINowcastingDateSeriesView(rnc, vm.ShowNowcasting, vm.ShowNowcasting7Day);
            if(vm.ShowNowcasting || vm.ShowNowcasting7Day) {
                Progress pgrRnc = new Progress(65, 30);
                pgrRnc.Changed += _pgr_Changed;
                await vRnc.CalcAsync(vm.Start, vm.End, pgrRnc);
                pgrRnc.Changed -= _pgr_Changed;
            }

            // 4. Update Title
            cht.Titles[0].Text = vm.Country;

            // 5. Add strip lines for weekend
            cht.ChartAreas[0].AxisX.StripLines.Clear();
            cht.ChartAreas[0].AxisX.StripLines.Add(new StripLine { IntervalOffset = -0.5d - (int)vm.Start.DayOfWeek,
                                                                   Interval = 7,
                                                                   StripWidth = 2,
                                                                   BackColor = Color.FromArgb(247, 247, 247) });

            // 6. Show available series
            cht.Series.Clear();
            _pgr.Report(96);
            cht.Series.Add(vSeir);
            _pgr.Report(97);
            cht.Series.Add(vRnc);
            _pgr.Report(98);
            cht.Series.Add(vJhu);
            _pgr.Report(99);

            cht.ChartAreas[0].RecalculateAxesScale();

            _pgr.Report(100);

            // 7. Add Legend
            cht.Legends.Clear();
            cht.Legends.Add(new Legend("Corona SEIR") { Font = cht.ChartAreas[0].AxisX.TitleFont });
            
            _pgr.Report(100, "Ready", false);

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
        private async Task UpdatePopulationAsync(WorkbenchViewModel vm) => vm.Population = await new WPPopulation().GetDataAsync(vm.Country, DateTime.Today.Year - 1);

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
            Dispatcher.BeginInvoke((Action)(async () => await new JHU().LoadAsync()), DispatcherPriority.Background);

            InitChart();
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => { if(vm.SolveR0 || vm.SolveR0Interval)
                                                                                            await UpdateReproductionAsync(vm);
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

            pbr.Dispatcher.Invoke(delegate () { }, DispatcherPriority.Background);
        }

        private void btnData_Click(object sender, RoutedEventArgs e) {
            new Data(cht.ToDataTable()).ShowDialog();
        }

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

        private void btnReset_Click(object sender, RoutedEventArgs e) {
            if(!(this.DataContext is WorkbenchViewModel vm)) return;

            _bUpdateReproduction = true;
            vm.Reset();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e) {
            if(!(this.DataContext is WorkbenchViewModel vm)) return;

            _bUpdateReproduction = true;
            vm.Clear();
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
                _tmr.Stop();
                Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => { if(_bUpdateReproduction)
                                                                                                await UpdateReproductionAsync(this.DataContext as WorkbenchViewModel);
                                                                                             await UpdateChartAsync(this.DataContext as WorkbenchViewModel); 
                                                                                           }));
            }
        }

        #endregion

        #region ViewModel Events

        private void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(!(sender is WorkbenchViewModel vm)) return;

            switch(e.PropertyName) {
                case nameof(WorkbenchViewModel.Country):
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => { _bUpdating = true;
                                                                                                 await UpdatePopulationAsync(vm); 
                                                                                                 await UpdateInfectiousAsync(vm);
                                                                                                 _bUpdating = false;
                                                                                                 _bUpdateReproduction = true;
                                                                                                 _tmr.Start();
                                                                                               }));
                    break;

                case nameof(WorkbenchViewModel.Start):
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(async () => { _bUpdating = true;
                                                                                                 await UpdateInfectiousAsync(vm);
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
                    _bUpdateReproduction = true;
                    _tmr.Start();
                    break;

                case nameof(WorkbenchViewModel.Reproduction):
                    if(!_bUpdating)
                        _tmr.Start();
                    break;                
                
                default:
                    _tmr.Start();
                    break;
            }
        }

        #endregion
    }
}
