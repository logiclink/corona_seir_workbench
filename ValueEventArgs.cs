using System;

namespace LogicLink.Corona {

    /// <summary>
    /// <see cref="EventArgs"/> with a value of type T
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class ValueEventArgs<T> : EventArgs {

        /// <summary>
        /// EventArgs value
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Creates a new <see cref="EventArgs"/>-object
        /// </summary>
        /// <param name="value"></param>
        public ValueEventArgs(T value) => this.Value = value;

    }
}
