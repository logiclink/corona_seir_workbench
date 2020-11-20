using System;

namespace LogicLink.Corona {
    
    /// <summary>
    /// Progress class with changed event
    /// </summary>
    public class Progress : IProgress<int> {

        /// <summary>
        /// Fired if value of progress changed.
        /// </summary>
        public event EventHandler<ValueEventArgs<(int, string, bool)>> Changed;

        /// <summary>
        /// Method to call the <see cref="Changed"/>-event
        /// </summary>
        /// <param name="iProgress">Current progress between a base value and a base value plus a range.</param>
        /// <param name="sMessage">Message to be shown.</param>
        /// <param name="bShow">True, if the progress should be displayed. False, if the progress should be hidden.</param>
        private void OnChanged(int iProgress, string sMessage, bool bShow) => this.Changed?.Invoke(this, new ValueEventArgs<(int Progress, string Message, bool Show)>((iProgress, sMessage, bShow)));

        private readonly int _iBase = 0;
        private readonly int _iRange = 100;

        #region Constructors

        /// <summary>
        /// Creates a new progress object
        /// </summary>
        public Progress() { }

        /// <summary>
        /// Creates and initialized a new progress object.
        /// </summary>
        /// <param name="iBase">Minimal value of the progress</param>
        /// <param name="iRange">Range of the progress-value starting from the minimal value.</param>
        public Progress(int iBase, int iRange) {
            _iBase = iBase;
            _iRange = iRange;
        }

        #endregion

        /// <summary>
        /// Reports a new progress value
        /// </summary>
        /// <param name="value">Progress between 0 and 100.</param>
        public void Report(int value) => OnChanged(_iBase + _iRange * value / 100, default, true);

        /// <summary>
        /// Reports a new progress value with a message and visibility.
        /// </summary>
        /// <param name="value">Progress between 0 and 100.</param>
        /// <param name="sMessage">Message</param>
        /// <param name="bShow">True, if the progress should be displayed. False, if the progress should be hidden.</param>
        public void Report(int value, string sMessage, bool bShow = false) => OnChanged(_iBase + _iRange * value / 100, sMessage, bShow);

    }
}
