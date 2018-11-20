using Guytp.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace Guytp.Data
{
    /// <summary>
    /// A stored procedure data set reader is responsible for handling a SqlDataReader and then obtaining the appropriate rows of data from the stored procedure.
    /// </summary>
    public class StoredProcedureDataSetReader : IDisposable
    {
        #region Declarations
        /// <summary>
        /// Defines the logger to use in this object.
        /// </summary>
        private static readonly Logger Logger = Logger.ApplicationInstance;

        /// <summary>
        /// The underlying stored procedure to use.
        /// </summary>
        private readonly StoredProcedure _storedProcedure;

        /// <summary>
        /// Defines the underlying data reader to use.
        /// </summary>
        private IDataReader _dataReader;

        /// <summary>
        /// Defines the underlying command to use.
        /// </summary>
        private IDbCommand _dbCommand;

        /// <summary>
        /// Defines the current data set that we are processing.
        /// </summary>
        private int _currentDataSet = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of a stored procedure data reader.  Once this reader has been created it becomes responsible for disposing of the underlying data reader.
        /// </summary>
        /// <param name="storedProcedure">
        /// The underlying stored procedure to use.
        /// </param>
        /// <param name="dataReader">
        /// The underlying data reader to use.
        /// </param>
        /// <param name="dbCommand">
        /// The underlying command to use.
        /// </param>
        public StoredProcedureDataSetReader(StoredProcedure storedProcedure, IDataReader dataReader, IDbCommand dbCommand)
        {
            _dataReader = dataReader;
            _storedProcedure = storedProcedure;
            _dbCommand = dbCommand;
        }
        #endregion

        /// <summary>
        /// Gets a list of items from the next data set supplied as the specified type.
        /// </summary>
        /// <typeparam name="DataSetType">
        /// The type of object that is expected for this data set.
        /// </typeparam>
        /// <returns>
        /// A list of items from this data set.
        /// </returns>
        public DataSetType[] GetDataSetList<DataSetType>()
        {
            // Throw an error if we've finished processing
            if (_currentDataSet == -1)
                throw new NoMoreDataSetsAvailableException();

            // Process all rows
            List<DataSetType> results = new List<DataSetType>();
            while (_dataReader.Read())
                results.Add((DataSetType)_storedProcedure.ParseRow(_dataReader, _currentDataSet));

            // Attempt to move to next data set for next call
            if (_dataReader.NextResult())
                _currentDataSet++;
            else
                _currentDataSet = -1;
            Logger.Trace("Processed data set with " + results.Count + " rows and " + (_currentDataSet == -1 ? "no more data sets available from this reader" : "data remains to be processed"));

            // Return the results
            return results.ToArray();
        }

        /// <summary>
        /// Gets a single items from the next data set supplied as the specified type.  This can only be called a single time per data set - if more than one row is required then GetDataSetList should be called.
        /// </summary>
        /// <typeparam name="DataSetType">
        /// The type of object that is expected for this data set.
        /// </typeparam>
        /// <returns>
        /// A single item from this data set.
        /// </returns>
        public DataSetType GetDataSetRow<DataSetType>()
        {
            // Throw an error if we've finished processing
            if (_currentDataSet == -1)
                throw new NoMoreDataSetsAvailableException();

            // Process all rows
            DataSetType result = default(DataSetType);
            if (_dataReader.Read())
                result =  (DataSetType)_storedProcedure.ParseRow(_dataReader, _currentDataSet);

            // Attempt to move to next data set for next call
            if (_dataReader.NextResult())
                _currentDataSet++;
            else
                _currentDataSet = -1;
            Logger.Trace("Processed data set and " + (result == null ? "found now data" : "read a row") + ".  " + (_currentDataSet == -1 ? "No more data sets available from this reader." : "Data remains to be processed."));

            // Return the result
            return result;
        }

        /// <summary>
        /// Process a IDbCommand after execution to parse any output parameters in to the relevent StoredProcedure object.  This can only be called once and will close
        /// the associated reader first and then close the DB command.  This should be the very last action called against a data reader.
        /// </summary>
        public void ProcessOutputParameters()
        {
            // Ensure we haven't already called this, close the reader and dispose of it to gather output parameters
            if (_dataReader == null)
                throw new OutputParametersAlreadyProcessedException();
            _dataReader.Dispose();
            _dataReader = null;
            _currentDataSet = -1;

            // Process output parameters
            IDbDataParameter[] parameters = new IDbDataParameter[_dbCommand.Parameters.Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = (IDbDataParameter)_dbCommand.Parameters[i];
            _storedProcedure.ParseDataParametersForOutput(parameters);

            // We may as well free up our command too now
            _dbCommand.Dispose();
            _dbCommand = null;
        }

        #region IDisposable Implementation
        /// <summary>
        /// Free up our resources.
        /// </summary>
        public void Dispose()
        {
            _currentDataSet = -1;
            _dataReader?.Dispose();
            _dbCommand?.Dispose();
        }
        #endregion
    }
}