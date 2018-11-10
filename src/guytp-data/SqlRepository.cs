using System;
using System.Data;
using System.Data.SqlClient;

namespace Guytp.Data
{
    /// <summary>
    /// A SqlRepository is a type of repository that connects to a Microsoft SQL server.
    /// </summary>
    public abstract class SqlRepository : IDisposable
    {
        #region Declarations
        /// <summary>
        /// Defines the currently in-progress transaction.
        /// </summary>
        private IDbTransaction _transaction;

        /// <summary>
        /// Defines the connection string to use in this repository.
        /// </summary>
        private readonly string _connectionString;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current connection in use for the whole repository.
        /// </summary>
        protected IDbConnection Connection { get; set; }

        /// <summary>
        /// Gets whether or not a transaction is currently in progress.
        /// </summary>
        protected bool IsTransactionInProgress { get { return _transaction != null; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="connectionStringName">
        /// The connection string that this repository should use.
        /// </param>
        public SqlRepository(string connectionStringName)
        {
            // Get a handle on the connection string
            _connectionString = DataConfig.ApplicationInstance.GetConnectionString(connectionStringName);
        }
        #endregion

        #region Connections and Commands
        /// <summary>
        /// Ensures a connection is open for this repository.
        /// </summary>
        protected virtual void EnsureOpenConnection()
        {
            if (Connection == null)
                Connection = CreateConnection();
            if (Connection == null)
                throw new DatabaseConnectionUnavailableException();
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                if (Connection.State != ConnectionState.Open)
                    throw new DatabaseConnectionUnavailableException();
            }
            catch (Exception ex)
            {
                throw new DatabaseConnectionUnavailableException(ex);
            }
        }

        /// <summary>
        /// Creates a new SQL connection.
        /// </summary>
        /// <returns>
        /// A new connection in its default state.
        /// </returns>
        protected virtual IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Creates a new IDbCommand from the repository-wide connection and opens a connection if not already done.
        /// </summary>
        /// <returns>
        /// A new DB command entered in to any in-progress transaction.
        /// </returns>
        protected virtual IDbCommand CreateCommand()
        {
            EnsureOpenConnection();
            IDbCommand command = Connection.CreateCommand();
            if (_transaction != null)
                command.Transaction = _transaction;
            return command;
        }
        #endregion

        #region Transactions
        /// <summary>
        /// Begin a new transaction.  If no connection has been established then one is now.
        /// </summary>
        protected void TransactionBegin()
        {
            // Ensure no existing transaction
            if (IsTransactionInProgress)
                throw new TransactionAlreadyInProgressException();

            // Ensure a connection then begin a transaction
            EnsureOpenConnection();
            _transaction = Connection.BeginTransaction();
        }

        /// <summary>
        /// Commit an in progress transaction.
        /// </summary>
        protected void TransactionCommit()
        {
            // Ensure an existing transaction
            if (!IsTransactionInProgress)
                throw new NoTransactionInProgressException();

            // Commit our transaction
            _transaction.Commit();
            _transaction = null;
        }

        /// <summary>
        /// Rolls back an in progress transaction.
        /// </summary>
        protected void TransactionRollback()
        {
            // Ensure an existing transaction
            if (!IsTransactionInProgress)
                throw new NoTransactionInProgressException();

            // Rollback our transaction
            _transaction.Rollback();
            _transaction = null;
        }
        #endregion

        #region Stored Procedures
        /// <summary>
        /// Executes a stored procedure that returns no data.
        /// </summary>
        /// <param name="storedProcedure">
        /// The stored procedure to execute.
        /// </param>
        protected void ExecuteProcedureNonQuery(StoredProcedure storedProcedure)
        {
            using (IDbCommand command = CreateStoredProcedureCommand(storedProcedure))
            {
                command.ExecuteNonQuery();
                ProcessCommandOutputParameters(command, storedProcedure);
            }
        }

        /// <summary>
        /// Execute a stored procedure and return a single scalar value.
        /// </summary>
        /// <typeparam name="ScalarType">
        /// The type of value to return.
        /// </typeparam>
        /// <param name="storedProcedure">
        /// The stored procedure to execute.
        /// </param>
        /// <returns>
        /// An object of ScalarType.
        /// </returns>
        protected ScalarType ExecuteProcedureScalar<ScalarType>(StoredProcedure storedProcedure)
        {
            using (IDbCommand command = CreateStoredProcedureCommand(storedProcedure))
            {
                object result = command.ExecuteScalar();
                ProcessCommandOutputParameters(command, storedProcedure);
                return result == DBNull.Value ? default(ScalarType) : (ScalarType)result;
            }
        }

        /// <summary>
        /// Execute a stored procedure and return a StoredProcedureDataSetReader.
        /// </summary>
        /// <param name="storedProcedure">
        /// The stored procedure to execute.
        /// </param>
        /// <returns>
        /// A StoredProcedureDataSetReader that is ready to read the output of the procedure..
        /// </returns>
        protected StoredProcedureDataSetReader ExecuteProcedureReader(StoredProcedure storedProcedure)
        {
            IDbCommand command = null;
            IDataReader reader = null;
            try
            {
                command = CreateStoredProcedureCommand(storedProcedure);
                reader = command.ExecuteReader();
                ProcessCommandOutputParameters(command, storedProcedure);
                return new StoredProcedureDataSetReader(storedProcedure, reader, command);
            }
            catch
            {
                if (reader != null)
                    reader.Dispose();
                if (command != null)
                    command.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Create a new IDbCommand for executing the specified stored procedure object in its current state.
        /// </summary>
        /// <param name="storedProcedure">
        /// The stored procedure to execute.
        /// </param>
        /// <returns>
        /// A new IDbCommand connected to the current open connection ready for execution.
        /// </returns>
        private IDbCommand CreateStoredProcedureCommand(StoredProcedure storedProcedure)
        {
            // Create and return command with input parameters and other setup
            IDbCommand command = CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedure.ProcedureName;
            if (storedProcedure.Timeout.HasValue)
                command.CommandTimeout = storedProcedure.Timeout.Value;
            IDataParameter[] parameters = storedProcedure.GenerateDataParameters();
            if (parameters != null)
                foreach (IDataParameter parameter in parameters)
                    command.Parameters.Add(parameter);
            return command;
        }

        /// <summary>
        /// Process a IDbCommand after execution to parse any output parameters in to the relevent StoredProcedure object.
        /// </summary>
        /// <param name="command">
        /// The command that has been executed.
        /// </param>
        /// <param name="storedProcedure">
        /// The procedure that needs to set output property values.
        /// </param>
        private void ProcessCommandOutputParameters(IDbCommand command, StoredProcedure storedProcedure)
        {
            IDbDataParameter[] parameters = new IDbDataParameter[command.Parameters.Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = (IDbDataParameter)command.Parameters[i];
            storedProcedure.ParseDataParametersForOutput(parameters);
        }
        #endregion

        #region IDisposable Implementation
        /// <summary>
        /// Free up our resources.
        /// </summary>
        public void Dispose()
        {
            if (_transaction != null)
                try
                {
                    TransactionRollback();
                }
                catch
                {
                    // Intentionally swallowed during dispose
                }
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }
        #endregion
    }
}