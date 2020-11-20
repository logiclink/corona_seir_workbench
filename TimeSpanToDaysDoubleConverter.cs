using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace LogicLink.Corona {

    /// <summary>
    /// Converter that converts a TimeSpan in a double.
    /// </summary>
    [ValueConversion(typeof(TimeSpan), typeof(double))]
    public class TimeSpanToDaysDoubleConverter : IValueConverter {

        /// <summary>
        /// Return the converter as a singleton instance
        /// </summary>
        /// <remarks>
        /// The method is thread-safe und not lazy, see http://csharpindepth.com/Articles/General/Singleton.aspx
        /// </remarks>
        public static IValueConverter Instance { get; } = new TimeSpanToDaysDoubleConverter();

        #region IValueConverter Members
        /// <summary>
        /// Converts a TimeSpan into a double
        /// </summary>
        /// <param name="value">TimeSpan</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Prefix string. Multiple prefixes can be separated by "|".</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>double</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is TimeSpan ts ? ts.TotalDays : DependencyProperty.UnsetValue;

        /// <summary>
        /// Converts a double of days into a TimeSpan
        /// </summary>
        /// <param name="value">Double</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Prefix string. Multiple prefixes can be separated by "|".</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>TimeSpan</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => double.TryParse(value.ToString(), NumberStyles.Any, culture, out double d) ? TimeSpan.FromDays(d) : DependencyProperty.UnsetValue;

        #endregion
    }
}