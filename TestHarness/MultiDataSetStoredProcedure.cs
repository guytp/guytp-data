using System;

namespace Guytp.Data.TestHarness
{
    /// <summary>
    /// This stored procedure represents a test to the data framework that returns multiple data sets.
    /// </summary>
    public class MultiDataSetStoredProcedure : StoredProcedure
    {
        #region Properties
        /// <summary>
        /// Gets a text input parameter.
        /// </summary>
        [StoredProcedureParameter]
        public string StringInput { get; }

        /// <summary>
        /// Gets the a test Guid output value from the procedure.
        /// </summary>
        [StoredProcedureParameter(System.Data.ParameterDirection.Output)]
        public Guid GuidOutput { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="input">
        /// The input parameter to pass to the stored procedure.
        /// </param>
        public MultiDataSetStoredProcedure(string input)
            : base("TestMultiDataSet", new[] { typeof(TestRowOne), typeof(TestRowTwo), typeof(TestRowThree) } )
        {
            StringInput = input;
        }
        #endregion
    }
}