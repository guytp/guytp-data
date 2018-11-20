using System;

namespace Guytp.Data
{
    /// <summary>
    /// This exception indicates a supplied connection string name could not be found.
    /// </summary>
    public class ConnectionStringNotFoundException : Exception
    {
        #region Constructors
        /// <summary>
        /// Create a new instnace of this class.
        /// </summary>
        /// <param name="connectionStringName">
        /// The name of the connection string that was not found.
        /// </param>
        internal ConnectionStringNotFoundException(string connectionStringName)
            : base("Connection string not found with the name " + connectionStringName)
        {
        }
        #endregion
    }
}