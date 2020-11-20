using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogicLink.Corona {

    /// <summary>
    /// Converts the values Visibility.Collapsed or Visibility.Hidden into a bool value.
    /// </summary>
    [ValueConversion(typeof(Visibility), typeof(bool?))]
    public class VisibilityToNotBooleanConverter : IValueConverter {

        /// <summary>
        /// Return the converter as a singleton instance
        /// </summary>
        /// <remarks>
        /// The method is thread-safe und not lazy, see http://csharpindepth.com/Articles/General/Singleton.aspx
        /// </remarks>
        public static IValueConverter Instance { get; } = new VisibilityToNotBooleanConverter();

        /// <summary>
        /// Converts a <see cref="Visibility"/> into a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Visibility v ? v != Visibility.Visible : Binding.DoNothing;

        /// <summary>
        /// Converts a <see cref="bool"/> into a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is bool b)
                return b ? Visibility.Collapsed : Visibility.Visible;
            else
                return Binding.DoNothing;
        }
    }

}
