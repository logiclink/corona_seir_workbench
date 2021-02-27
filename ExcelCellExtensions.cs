using System.Linq;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace LogicLink.Corona {

    /// <summary>
    /// DocumentFormat.OpenXml.Spreadsheet extensions for cells
    /// </summary>
    public static class ExcelCellExtensions {

        /// <summary>
        /// Returns the value of a cell as string
        /// </summary>
        /// <param name="c">Cell</param>
        /// <param name="wpPart">WorksheetPart for shared string table</param>
        /// <returns>String representation of the value.</returns>
        /// <remarks>
        /// see https://docs.microsoft.com/de-de/office/open-xml/how-to-retrieve-the-values-of-cells-in-a-spreadsheet
        /// </remarks>
        public static string GetValue(this Cell c, WorkbookPart wpPart = null) {
            string s = c.InnerText;

            // If the cell represents an integer number, you are done. 
            // For dates, this code returns the serialized value that 
            // represents the date. The code handles strings and 
            // Booleans individually. For shared strings, the code 
            // looks up the corresponding value in the shared string 
            // table. For Booleans, the code converts the value into 
            // the words TRUE or FALSE.
            if(c.DataType != null) {
                switch(c.DataType.Value) {
                    case CellValues.SharedString:

                        // For shared strings, look up the value in the
                        // shared strings table.
                        SharedStringTablePart stp = wpPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                        // If the shared string table is missing, something 
                        // is wrong. Return the index that is in
                        // the cell. Otherwise, look up the correct text in 
                        // the table.
                        if(stp != null)
                            s = stp.SharedStringTable.ElementAt(int.Parse(s)).InnerText;
                        break;

                    case CellValues.Boolean:
                        switch(s) {
                            case "0":
                                s = "FALSE";
                                break;

                            default:
                                s = "TRUE";
                                break;
                        }
                        break;
                }
            }
            return s;
        }
    }
}
