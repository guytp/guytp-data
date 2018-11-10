using System;

namespace Guytp.Data
{
    /// <summary>
    /// This exception indicates that a transaction is already running on a DB connection when a new one has been requested.
    /// </summary>
    public class TransactionAlreadyInProgressException : Exception
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        internal TransactionAlreadyInProgressException()
            : base("There is an existing transaction already in progress")
        {
        }
        #endregion
    }
}