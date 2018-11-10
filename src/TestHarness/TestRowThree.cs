using System;

namespace Guytp.Data.TestHarness
{
    /// <summary>
    /// This class represents a dummy row of data.
    /// </summary>
    internal class TestRowThree
    {
        #region Constructors
        /// <summary>
        /// Gets the test string value.
        /// </summary>
        public string StringValue { get; }

        /// <summary>
        /// Gets the test integer value.
        /// </summary>
        public int IntValue { get; }

        /// <summary>
        /// Gets the test GUID value.
        /// </summary>
        public Guid GuidValue { get; }

        /// <summary>
        /// Gets the test enum value.
        /// </summary>
        public TestEnum TestEnum { get; }
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
        /// <param name="enumValue">
        /// The test GUID value.
        /// </param>
        [StoredProcedureDataSetConstructor]
        public TestRowThree(string stringValue, int intValue, Guid guidValue, TestEnum enumValue)
        {
            StringValue = stringValue;
            IntValue = intValue;
            GuidValue = guidValue;
            TestEnum = enumValue;
        }
        #endregion
    }
}