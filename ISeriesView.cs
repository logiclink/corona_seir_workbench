using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {

    /// <summary>
    /// Interface for an enumerable of charting series. Series can be calculated for a day.
    /// </summary>
    public interface ISeriesView : IEnumerable<Series> {

        /// <summary>
        /// Calculates the model for a number of days
        /// </summary>
        /// <param name="iDays">Number of days</param>
        /// <returns>Task</returns>
        Task CalcAsync(int iDays);

    }
}
