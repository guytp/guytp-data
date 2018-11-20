using System;

namespace Guytp.Data
{
    /// <summary>
    /// This exception indicates that an underlying database connection is unavailble when it was expected and required to be available.
    /// </summary>
    public class DatabaseConnectionUnavailableException : Exception
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="innerException">
        /// The optional inner exception triggering this exception.
        /// </param>
        internal DatabaseConnectionUnavailableException(Exception innerException = null)
            : base ("No connection to the underlying database is unavailable", innerException)
        {
        }
        #endregion
    }
}