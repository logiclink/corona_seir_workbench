using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace LogicLink.Corona {

    /// <summary>
    /// Interface for an enumerable of charting series. Series can be calculated for a time range.
    /// </summary>
    public interface IDateSeriesView : IEnumerable<Series> {
        
        /// <summary>
        /// Calculates the model for a number of days between a start and an end date
        /// </summary>
        /// <param name="dtStart">Start date</param>
        /// <param name="dtEnd">End date</param>
        /// <returns>Task</returns>
        Task CalcAsync(DateTime dtStart, DateTime dtEnd, IProgress<int> p);

    }
}
