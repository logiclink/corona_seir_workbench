using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Corona {

    /// <summary>
    /// Helper Functions for DateTime values
    /// </summary>
    public static class DateTimeHelper {

        /// <summary>
        /// Returns the maximum of two DateTime values
        /// </summary>
        /// <param name="dt1">First DateTime value</param>
        /// <param name="dt2">Second DateTime value</param>
        /// <returns>Maximum DateTime value</returns>
        public static DateTime Max(DateTime dt1, DateTime dt2) => dt1 > dt2 ? dt1 : dt2;

        /// <summary>
        /// Returns the minimum of two DateTime values
        /// </summary>
        /// <param name="dt1">First DateTime value</param>
        /// <param name="dt2">Second DateTime value</param>
        /// <returns>Minimum DateTime value</returns>
        public static DateTime Min(DateTime dt1, DateTime dt2) => dt1 < dt2 ? dt1 : dt2;

        /// <summary>
        /// Returns the maximum of two nullable DateTime values. Null values are
        /// ignored and the not null DateTime value will be returned if possible.
        /// </summary>
        /// <param name="dt1">First nullable DateTime value</param>
        /// <param name="dt2">Second nullable DateTime value</param>
        /// <returns>Maximum DateTime value or null if both DateTime values are null.</returns>
        public static DateTime? Max(DateTime? dt1, DateTime? dt2) => dt1 == null ? dt2 : dt2 == null ? dt1 : dt1 > dt2 ? dt1 : dt2;

        /// <summary>
        /// Returns the minimum of two nullable DateTime values. Null values are
        /// ignored and the not null DateTime value will be returned if possible.
        /// </summary>
        /// <param name="dt1">First nullable DateTime value</param>
        /// <param name="dt2">Second nullable DateTime value</param>
        /// <returns>Minimum DateTime value or null if both DateTime values are null.</returns>
        public static DateTime? Min(DateTime? dt1, DateTime? dt2) => dt1 == null ? dt2 : dt2 == null ? dt1 : dt1 < dt2 ? dt1 : dt2;
    }
}
