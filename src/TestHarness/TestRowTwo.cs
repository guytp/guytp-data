namespace Guytp.Data.TestHarness
{
    /// <summary>
    /// This class represents a dummy row of data.
    /// </summary>
    internal class TestRowTwo
    {
        #region Constructors
        /// <summary>
        /// Gets the first test string value.
        /// </summary>
        public string StringOne { get; }

        /// <summary>
        /// Gets the secon test string value.
        /// </summary>
        public string StringTwo { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="stringOne">
        /// The first test string value.
        /// </param>
        /// <param name="stringTwo">
        /// The second test string value.
        /// </param>
        [StoredProcedureDataSetConstructor]
        public TestRowTwo(string stringOne, string stringTwo)
        {
            StringOne = stringOne;
            StringTwo = stringTwo;
        }
        #endregion
    }
}