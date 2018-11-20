using System;

namespace Guytp.Data
{
    /// <summary>
    /// This exception indicates that a StoredProcedureDataSetReader has been asked to process output parameters for a second time when only one reading is permitted.
    /// </summary>
    public class OutputParametersAlreadyProcessedException : Exception
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        internal OutputParametersAlreadyProcessedException()
            : base("Output parameters from the stored procedure have already been processed")
        {
        }
        #endregion
    }
}