using System;

namespace Guytp.Data.TestHarness
{
    /// <summary>
    /// The test repository is designing for testing the data framework.
    /// </summary>
    internal class TestRepository : SqlRepository
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        public TestRepository()
            : base("Test")
        {
        }
        #endregion

        /// <summary>
        /// Return a composite object of various data.
        /// </summary>
        /// <param name="inputValue">
        /// The input value to pass to the database.
        /// </param>
        /// <returns>
        /// A new CompositeResultObject containing the data.
        /// </returns>
        internal CompositeResultObject GetCompositeResult(string inputValue)
        {
            MultiDataSetStoredProcedure storedProcedure = new MultiDataSetStoredProcedure(inputValue);
            using (StoredProcedureDataSetReader reader = ExecuteProcedureReader(storedProcedure))
            {
                TestRowOne[] rowOnes = reader.GetDataSetList<TestRowOne>();
                TestRowTwo[] rowTwos = reader.GetDataSetList<TestRowTwo>();
                TestRowThree[] rowThrees = reader.GetDataSetList<TestRowThree>();
                reader.ProcessOutputParameters();
                return new CompositeResultObject(rowOnes, rowTwos, rowThrees, storedProcedure.GuidOutput);
            }
        }

        /// <summary>
        /// Return a Guid from the output parameter of calling the MultiDataSetStoredProcedure as Scalar.
        /// </summary
        /// <returns>
        /// A Guid from the output parameter of calling the MultiDataSetStoredProcedure as Scalar.
        /// </returns>
        internal Guid GetTestOutputGuidFromScalar()
        {
            MultiDataSetStoredProcedure storedProcedure = new MultiDataSetStoredProcedure("test");
            string result = ExecuteProcedureScalar<string>(storedProcedure);
            return storedProcedure.GuidOutput;
        }
        /// <summary>
        /// Return a Guid from the output parameter of calling the MultiDataSetStoredProcedure as non-query.
        /// </summary
        /// <returns>
        /// A Guid from the output parameter of calling the MultiDataSetStoredProcedure as non-query.
        /// </returns>
        internal Guid GetTestOutputGuidFromNonQuery()
        {
            MultiDataSetStoredProcedure storedProcedure = new MultiDataSetStoredProcedure("test");
            ExecuteProcedureNonQuery(storedProcedure);
            return storedProcedure.GuidOutput;
        }
    }
}