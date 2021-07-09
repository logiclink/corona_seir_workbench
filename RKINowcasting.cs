using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace LogicLink.Corona {

    /// <summary>
    /// Basic reproduction numbers (R₀) per date for Germany from the Robert Koch-Institut 
    /// </summary>
    public class RKINowcasting {

        private const string RKI_NOWCASTING_URL = "https://raw.githubusercontent.com/robert-koch-institut/SARS-CoV-2-Nowcasting_und_-R-Schaetzung/main/Nowcast_R_aktuell.csv";

        /// <summary>
        /// Structure for a record of an Excel file row
        /// </summary>
        public struct Record {

            /// <summary>
            /// Datum des Erkrankungsbeginns (Column A)
            /// </summary>
            public readonly DateTime Date;

            /// <summary>
            /// Punktschätzer der Reproduktionszahl R₀ (Column H)
            /// </summary>
            public readonly double Reproduction;

            /// <summary>
            /// Punktschätzer des 7-Tage-R₀ Wertes (Column K)
            /// </summary>
            public readonly double Reproduction7Day;

            #region Constructors

            /// <summary>
            /// Creates and initializes a new record
            /// </summary>
            /// <param name="s">Row of the RKI R₀ csv data file as string.</param>
            public Record(string s) {
                ReadOnlySpan<char> sp = s;
                int i = 0;

                // Date of the R₀-value
                int j = sp[i..].QuotedIndexOf(',');
                if(!DateTime.TryParse(sp.Slice(i, j), CultureInfo.InvariantCulture, DateTimeStyles.None, out Date))
                    Date = DateTime.MinValue;
                i += j + 1;

                // Ignore the next six columns with case numbers
                for(int k = 0; k < 6; k++) {
                    j = sp[i..].QuotedIndexOf(',');
                    i += j + 1;
                }

                // 4-day R₀-value
                j = sp[i..].QuotedIndexOf(',');
                if(!double.TryParse(sp.Slice(i, j), NumberStyles.Float, CultureInfo.InvariantCulture, out Reproduction))
                    Reproduction = 0d;
                i += j + 1;

                // Ignore the next two columns with lower and upper bounds of the 4-days R₀ value
                for(int k = 0; k < 2; k++) {
                    j = sp[i..].QuotedIndexOf(',');
                    i += j + 1;
                }

                // 7-day R₀-value
                j = sp[i..].QuotedIndexOf(',');
                if(!double.TryParse(sp.Slice(i, j), NumberStyles.Float, CultureInfo.InvariantCulture, out Reproduction7Day))
                    Reproduction7Day = 0d;
            }

            #endregion

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() => $"{Date} {Reproduction:N2} {Reproduction7Day:N2}";
        }

        /// <summary>
        /// Returns Robert Koch-Institut basic reproduction numbers (R₀) data per date for Germany
        /// </summary>
        /// <param name="sCountry">Name of the Country</param>
        /// <returns>Asynchronous enumerator</returns>
        public async IAsyncEnumerable<Record> GetDataAsync() {
            using(FileStream fs = new FileStream(await Download.GetCachedAsync(RKI_NOWCASTING_URL), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using(StreamReader rd = new StreamReader(fs)) {
                    await rd.ReadLineAsync();
                    while(!rd.EndOfStream) {
                        Record r = new Record(await rd.ReadLineAsync());
                        if(r.Date != DateTime.MinValue)
                            yield return r;
                }
            }
        }

    }
}
