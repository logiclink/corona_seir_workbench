using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace LogicLink.Corona {

    /// <summary>
    /// Basic reproduction numbers (R₀) per date for Germany from the Robert Koch-Institut 
    /// </summary>
    public class RKINowcasting {

        private const string RKI_NOWCASTING_URL = "https://www.rki.de/DE/Content/InfAZ/N/Neuartiges_Coronavirus/Projekte_RKI/Nowcasting_Zahlen.xlsx?__blob=publicationFile";

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
            /// <param name="spDate">Date of the record as <see cref="ReadOnlySpan<char>"/> or string</param>
            /// <param name="spReproduction">Basic reproduction number (R₀) as <see cref="ReadOnlySpan<char>"/> or string in an invariant number format</param>
            /// <param name="spReproduction7Day">Average of 7 days of the basic reproduction number (R₀) as <see cref="ReadOnlySpan<char>"/> or string in an invariant number format</param>
            public Record(ReadOnlySpan<char> spDate, ReadOnlySpan<char> spReproduction, ReadOnlySpan<char> spReproduction7Day) {
                if(!DateTime.TryParse(spDate, out Date) ||
                   !double.TryParse(spReproduction, NumberStyles.Any, CultureInfo.CurrentCulture, out Reproduction) ||
                   !double.TryParse(spReproduction7Day, NumberStyles.Any, CultureInfo.CurrentCulture, out Reproduction7Day)) {
                    Date = DateTime.MinValue;
                    Reproduction = float.MinValue;
                    Reproduction7Day = float.MinValue;
                }
            }

            #endregion

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() => $"{Date}\t{Reproduction:N2}";
        }

        /// <summary>
        /// Returns Robert Koch-Institut basic reproduction numbers (R₀) data per date for Germany
        /// </summary>
        /// <param name="sCountry">Name of the Country</param>
        /// <returns>Asynchronous enumerator</returns>
        public async IAsyncEnumerable<Record> GetDataAsync() {
            using(SpreadsheetDocument doc = SpreadsheetDocument.Open(await Download.GetCachedAsync(RKI_NOWCASTING_URL), false)) {
                WorkbookPart wbp = doc.WorkbookPart;
                Sheet sht = wbp.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == "Nowcast_R");
                WorksheetPart wsp = (WorksheetPart)(wbp.GetPartById(sht.Id));
                SheetData dta = wsp.Worksheet.Elements<SheetData>().First();
                foreach(Row row in dta.Elements<Row>()) {
                    List<Cell> cells = new List<Cell>(row.Elements<Cell>());
                    if(cells.Count > 10) {

                        // Column A (0):  Datum des Erkrankungsbeginns
                        // Column H (7):  Punktschätzer der Reproduktionszahl R
                        // Column K (10): Punktschätzer des 7-Tage-R Wertes
                        Record rec = new Record(cells[0].GetValue(wbp), cells[7].GetValue(wbp), cells[10].GetValue(wbp));

                        if(rec.Date != DateTime.MinValue)
                            yield return rec;
                    }
                }
            }
        }

    }
}
