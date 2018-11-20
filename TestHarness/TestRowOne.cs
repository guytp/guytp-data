namespace Guytp.Data.TestHarness
{
    /// <summary>
    /// This class represents a dummy row of data.
    /// </summary>
    internal class TestRowOne
    {
        #region Constructors
        /// <summary>
        /// Gets the test string value.
        /// </summary>
        public string StringValue { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="stringValue">
        /// The test string value.
        /// </param>
        /// <param name="intValue">
        /// The test integer value.
        /// </param>
        /// <param name="guidValue">
        /// The test GUID value.
        /// </param>
        [StoredProcedureDataSetConstructor]
        public TestRowOne(string stringValue)
        {
            StringValue = stringValue;
        }
        #endregion
    }
}