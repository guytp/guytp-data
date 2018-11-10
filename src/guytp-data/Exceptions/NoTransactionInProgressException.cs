using System;

namespace Guytp.Data
{
    /// <summary>
    /// This exception indicates that no transaction is running on a connection when a request to modify it has been made.
    /// </summary>
    public class NoTransactionInProgressException : Exception
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        internal NoTransactionInProgressException()
            : base("No transaction is running on the specified connection")
        {
        }
        #endregion
    }
}