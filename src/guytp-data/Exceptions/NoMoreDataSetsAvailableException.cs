using System;

namespace Guytp.Data
{
    /// <summary>
    /// This exception is used to indicate that no further data sets are available in the underlying data set, yet one has been requested for processing.
    /// </summary>
    public class NoMoreDataSetsAvailableException : Exception
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        internal NoMoreDataSetsAvailableException()
            : base("No more data sets are available to process in the underlying reader")
        {
        }
        #endregion
    }
}