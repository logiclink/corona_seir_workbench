using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {

    /// <summary>
    /// Extension methods for charts from System.Windows.Forms.DataVisualization.Chartin
    /// </summary>
    public static class ChartExtensions {

        /// <summary>
        /// Date component of a DateTime type
        /// </summary>
        /// <remarks>
        /// The IComparable interface is neccessary for sorting of the DataTable column in <see cref="DataGrid"/>.
        /// </remarks>
        private struct Date : IComparable {
            public readonly DateTime Value;
            public Date(DateTime dt) => this.Value = dt;

            public override string ToString() => this.Value.ToString("d");
            public string ToString(IFormatProvider provider) => this.Value.ToString("d", provider);
            public string ToString(string format) => this.Value.ToString(format);
            public string ToString(string format, IFormatProvider provider) => this.Value.ToString(format, provider);

            #region IComparable
            public int CompareTo(object obj) => Value.CompareTo((obj is Date d) ? d.Value : default);
            #endregion
        }


        /// <summary>
        /// Time component of a DateTime type
        /// </summary>
        /// <remarks>
        /// The IComparable interface is neccessary for sorting of the DataTable column in a WPF <see cref="DataGrid"/>.
        /// </remarks>
        private struct Time : IComparable {
            public readonly DateTime Value;
            public Time(DateTime dt) => this.Value = dt;
            public override string ToString() => this.Value.ToString("t");
            public string ToString(IFormatProvider provider) => this.Value.ToString("t", provider);
            public string ToString(string format) => this.Value.ToString(format);
            public string ToString(string format, IFormatProvider provider) => this.Value.ToString(format, provider);

            #region IComparable
            public int CompareTo(object obj) => Value.CompareTo((obj is Time d) ? d.Value : default);
            #endregion

        }

        /// <summary>
        /// Converts a ChartValueType into a Type
        /// </summary>
        /// <param name="cvt">ChartValueType</param>
        /// <returns>Type</returns>
        private static Type ToType(this ChartValueType cvt) => cvt switch {
            ChartValueType.Date => typeof(Date),
            ChartValueType.DateTime => typeof(DateTime),
            ChartValueType.Time => typeof(Time),
            ChartValueType.DateTimeOffset => typeof(DateTimeOffset),
            ChartValueType.Double => typeof(double),
            ChartValueType.Int32 => typeof(int),
            ChartValueType.Int64 => typeof(long),
            ChartValueType.Single => typeof(float),
            ChartValueType.String => typeof(string),
            ChartValueType.UInt32 => typeof(uint),
            ChartValueType.UInt64 => typeof(ulong),
            _ => typeof(object),
        };

        /// <summary>
        /// Writes a string to a <see cref="StreamWriter"/> and escapes the string with leading and closing double quotes if neccessary
        /// </summary>
        /// <param name="w">Stream writeer</param>
        /// <param name="sEscape">Escape, if these characters occur</param>
        /// <param name="s">String to write</param>
        /// <returns>Awaitable Task</returns>
        private static async Task WriteEscapedAsync(this StreamWriter w, string sEscape, string s) {
            if(string.IsNullOrEmpty(s)) return;
            if(s.Contains(sEscape, StringComparison.InvariantCulture)) {
                await w.WriteAsync('"');
                await w.WriteAsync(s);
                await w.WriteAsync('"');
            } else {
                await w.WriteAsync(s);
            }
        }

        /// <summary>
        /// Writes an array of objects to a line with a <see cref="StreamWriter"/> and escapes objects with leading and closing double quotes if neccessary
        /// </summary>
        /// <param name="w">Stream writeer</param>
        /// <param name="sEscape">Escape, if these characters occur</param>
        /// <param name="args">Parmaeter array</param>
        /// <returns>Awaitable Task</returns>
        private static async Task WriteEscapedLineAsync(this StreamWriter w, string sEscape, params object[] args) {
            if(args.Length != 0) {
                await w.WriteEscapedAsync(sEscape, args[0]?.ToString());
                for(int i = 1; i < args.Length; i++) {
                    await w.WriteAsync(';');
                    await w.WriteEscapedAsync(sEscape, args[i]?.ToString());
                }
            }
            await w.WriteAsync(w.NewLine);
        }

        /// <summary>
        /// Converts the series of a chart in to a DataTable
        /// </summary>
        /// <param name="cht">Chart with series</param>
        /// <returns><see cref="DataTable"/>  with x-values in the first column</returns>
        /// <remarks>
        /// Use the <see cref="DataTable"/> for the display of dynamic columns in a WPF <see cref="DataGrid"/>.
        /// </remarks>
        public static DataTable ToDataTable(this Chart cht) {
            DataTable tbl = new DataTable();

            if(cht.Series.Count != 0) {
                DataColumn col = tbl.Columns.Add(!string.IsNullOrEmpty(cht.ChartAreas[0].AxisX.Title)
                                ? cht.ChartAreas[0].AxisX.Title
                                : cht.Series[0].XValueType == ChartValueType.Date ? "Date" : "X-Value");
                col.DataType = cht.Series[0].XValueType.ToType();
                Dictionary<object, object[]> dic = new Dictionary<object, object[]>();
                List<Series> lSeries = cht.Series.OrderBy(ser => ser.YAxisType).ToList();
                for(int i = 0; i < lSeries.Count; i++) {
                    Series ser = lSeries[i];
                    col = tbl.Columns.Add(ser.Name);
                    col.DataType = ser.YValueType.ToType();
                    foreach(DataPoint pnt in ser.Points) {
                        object o = ser.XValueType switch
                        {
                            ChartValueType.Date => new Date(DateTime.FromOADate(pnt.XValue)),
                            ChartValueType.Time => new Time(DateTime.FromOADate(pnt.XValue)),
                            ChartValueType.DateTime => DateTime.FromOADate(pnt.XValue),
                            _ => Convert.ChangeType(pnt.XValue, ser.XValueType.ToType())
                        };
                        if(!dic.TryGetValue(o, out object[] ar)) {
                            ar = new object[cht.Series.Count + 1];
                            ar[0] = o;
                            dic.Add(o, ar);
                        }
                        ar[i + 1] = ser.YValueType switch
                        {
                            ChartValueType.Date => new Date(DateTime.FromOADate(pnt.YValues[0])),
                            ChartValueType.Time => new Time(DateTime.FromOADate(pnt.YValues[0])),
                            ChartValueType.DateTime => DateTime.FromOADate(pnt.YValues[0]),
                            _ => Convert.ChangeType(pnt.YValues[0], ser.YValueType.ToType())
                        };
                    }
                }

                foreach(object o in dic.Keys.OrderBy(o => o))
                    tbl.Rows.Add(dic[o]);

            }
            return tbl;
        }

        /// <summary>
        /// Saves the series of a chart in to a CSV-file.
        /// </summary>
        /// <param name="cht">Chart with series</param>
        /// <param name="sPath">File name with path</param>
        /// <param name="p">Progress object</param>
        /// <returns>Awaitable Task</returns>
        /// <remarks>
        /// Fields are separated by semicolon and surrounded by double quotes if the data contains a semicolon or line breaks.
        /// This format is expected e.g. by Microsoft Excel.
        /// </remarks>
        public static async Task ToCSVAsync(this Chart cht, string sPath, IProgress<int> p = null) {
            if(cht.Series.Count != 0)
                using(FileStream fs = File.Open(sPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    using(StreamWriter w = new StreamWriter(fs, Encoding.UTF8)) {
                        int iPCount = 0;

                        string sEscape = ";" + w.NewLine;
                        
                        await w.WriteEscapedAsync(sEscape, !string.IsNullOrEmpty(cht.ChartAreas[0].AxisX.Title)
                                                           ? cht.ChartAreas[0].AxisX.Title
                                                           : cht.Series[0].XValueType == ChartValueType.Date ? "Date" : "X-Value");
                        Dictionary<object, object[]> dic = new Dictionary<object, object[]>();
                        List<Series> lSeries = cht.Series.OrderBy(ser => ser.YAxisType).ToList();
                        for(int i = 0; i < lSeries.Count; i++) {
                            Series ser = lSeries[i];
                            await w.WriteAsync(';');
                            await w.WriteEscapedAsync(sEscape, ser.Name);
                            foreach(DataPoint pnt in ser.Points) {
                                object o = ser.XValueType switch
                                {
                                    ChartValueType.Date => new Date(DateTime.FromOADate(pnt.XValue)),
                                    ChartValueType.Time => new Time(DateTime.FromOADate(pnt.XValue)),
                                    ChartValueType.DateTime => DateTime.FromOADate(pnt.XValue),
                                    _ => Convert.ChangeType(pnt.XValue, ser.XValueType.ToType())
                                };
                                if(!dic.TryGetValue(o, out object[] ar)) {
                                    ar = new object[cht.Series.Count + 1];
                                    ar[0] = o;
                                    dic.Add(o, ar);
                                }
                                ar[i + 1] = ser.YValueType switch
                                {
                                    ChartValueType.Date => new Date(DateTime.FromOADate(pnt.YValues[0])),
                                    ChartValueType.Time => new Time(DateTime.FromOADate(pnt.YValues[0])),
                                    ChartValueType.DateTime => DateTime.FromOADate(pnt.YValues[0]),
                                    _ => Convert.ChangeType(pnt.YValues[0], ser.YValueType.ToType())
                                };
                            }
                        }
                        await w.WriteLineAsync();

                        int iP = 0;
                        foreach(object o in dic.Keys.OrderBy(o => o)) {
                            await w.WriteEscapedLineAsync(sEscape, dic[o]);
                            if(iPCount != 25 * ++iP / dic.Count) {
                                iPCount = 25 * iP / dic.Count;
                                p?.Report(4 * iPCount);
                            }
                        }

                        w.Flush();
                    }
        }
    }
}
