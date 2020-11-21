using System;
using System.Collections.Generic;
using System.Text;

namespace LogicLink.Corona {
    public static class ExceptionExtensions {

        /// <summary>
        /// Gets the most inner Exception
        /// </summary>
        /// <param name="e">Outer Exception</param>
        /// <returns>Exception</returns>
        public static Exception GetMostInnerException(this Exception ex) {
            Exception exi = ex;
            while(exi.InnerException != null)
                exi = exi.InnerException;
            return exi;
        }
    }
}
