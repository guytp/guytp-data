using System;

namespace Guytp.Data.TestHarness
{
    /// <summary>
    /// This class represents a composite object that a repository can return.
    /// </summary>
    internal class CompositeResultObject
    {
        #region Properties
        /// <summary>
        /// Gets the first row test data to include.
        /// </summary>
        public TestRowOne[] RowOnes { get; }

        /// <summary>
        /// Gets the second row test data to include.
        /// </summary>
        public TestRowTwo[] RowTwos { get; }

        /// <summary>
        /// Gets the third row test data to include.
        /// </summary>
        public TestRowThree[] RowThrees { get; }

        /// <summary>
        /// Gets the GUID returned from the database.
        /// </summary>
        public Guid Id { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="rowOnes">
        /// First row test data to include.
        /// </param>
        /// <param name="rowTwos">
        /// Second row test data to include.
        /// </param>
        /// <param name="rowThrees">
        /// Third row test data to include.
        /// </param>
        /// <param name="id">
        /// The ID returned from the database.
        /// </param>
        public CompositeResultObject(TestRowOne[] rowOnes, TestRowTwo[] rowTwos, TestRowThree[] rowThrees, Guid id)
        {
            Id = id;
            RowOnes = rowOnes;
            RowTwos = rowTwos;
            RowThrees = rowThrees;
        }
        #endregion
    }
}