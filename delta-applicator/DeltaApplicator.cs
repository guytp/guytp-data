using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Guytp.Data.DatabaseDeltaApplicator
{
    /// <summary>
    /// The delta applicator performs the main processing loop within the delta process and upgrades a database to the latest version.
    /// </summary>
    internal class DeltaApplicator
    {
        #region Declarations
        /// <summary>
        /// Defines an object containing the command line arguments passed in to the application.
        /// </summary>
        private readonly ArgumentParser _argumentParser;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="argumentParser">
        /// An object containing the command line arguments passed in to the application.
        /// </param>
        internal DeltaApplicator(ArgumentParser argumentParser)
        {
            if (argumentParser == null)
                throw new ArgumentNullException(nameof(argumentParser));
            _argumentParser = argumentParser;
        }
        #endregion

        /// <summary>
        /// Apply the deltas to the database.
        /// </summary>
        /// <returns>
        /// A DeltaApplicationResults object describing the application process.
        /// </returns>
        public DeltaApplicationResults Apply()
        {
            List<int> alreadyApplied = new List<int>();
            List<int> deltasToApply = new List<int>();
            List<int> successful = new List<int>();
            int? lastDelta = null;
            List<string> statusMessages = new List<string>();

            try
            {
                // Determine the deltas we need
                List<string> deltasToApplyCorrespondingFilenames = new List<string>();
                string[] potentialFiles = Directory.GetFiles(_argumentParser.DeltaPath, "*.sql");
                foreach (string potentialFile in potentialFiles)
                {
                    int parsedDeltaNumber;
                    string fileWithoutPath = Path.GetFileName(potentialFile);
                    string[] split = fileWithoutPath.Split(new char[] { ' ' }, 2);
                    if (!int.TryParse(split[0], out parsedDeltaNumber))
                        // Not a valid delta
                        continue;
                    deltasToApply.Add(parsedDeltaNumber);
                    deltasToApplyCorrespondingFilenames.Add(fileWithoutPath);
                }
                deltasToApply.Sort();

                using (SqlConnection connMaster = new SqlConnection("Server=" + _argumentParser.DatabaseServer + "; User Id=" + _argumentParser.DatabaseUsername + "; Password=" + _argumentParser.DatabasePassword + "; Database=master"))
                {
                    // First create database if it does not already exist
                    statusMessages.Add("Connecting to " + _argumentParser.DatabaseServer + " (master)");
                    Console.WriteLine(statusMessages[statusMessages.Count - 1]);
                    connMaster.Open();
                    SqlCommand comm = connMaster.CreateCommand();
                    comm.CommandType = CommandType.Text;
                    comm.CommandText = "SELECT 'Exists' FROM sys.databases WHERE [name] = @databaseName";
                    comm.Parameters.AddWithValue("@databaseName", _argumentParser.DatabaseName);
                    object response = comm.ExecuteScalar();
                    comm.Parameters.Clear();
                    if (response == null || response as string != "Exists")
                    {
                        comm.CommandText = "CREATE DATABASE [" + _argumentParser.DatabaseName + "]";
                        statusMessages.Add("Creating database " + _argumentParser.DatabaseName);
                        Console.WriteLine(statusMessages[statusMessages.Count - 1]);
                        comm.ExecuteNonQuery();
                    }
                    using (SqlConnection conn = new SqlConnection("Server=" + _argumentParser.DatabaseServer + "; User Id=" + _argumentParser.DatabaseUsername + "; Password=" + _argumentParser.DatabasePassword + "; Database=" + _argumentParser.DatabaseName))
                    {
                        statusMessages.Add("Connecting to " + _argumentParser.DatabaseServer + " ("+ _argumentParser.DatabaseName + ")");
                        Console.WriteLine(statusMessages[statusMessages.Count - 1]);
                        conn.Open();
                        comm = conn.CreateCommand();

                        // Now if we don't have the delta table, add it now
                        comm.CommandText = "SELECT 'Exists' FROM sys.tables WHERE [name] = 'DatabaseVersionInformation'";
                        response = comm.ExecuteScalar();
                        if (response == null || response as string != "Exists")
                        {
                            comm.CommandText = @"CREATE TABLE DatabaseVersionInformation
    (
        ChangesetName NVARCHAR(200) NOT NULL,
        DeltaNumber INT NOT NULL,
        Filename NVARCHAR(MAX) NOT NULL,
        ApplyStartTime DATETIME NOT NULL,
        ApplyCompleteTime DATETIME,
        CONSTRAINT PK_DatabaseVersionInformation PRIMARY KEY (ChangesetName, DeltaNumber)
    )";
                            statusMessages.Add("Creating DatabaseVersionInformation table");
                            Console.WriteLine(statusMessages[statusMessages.Count - 1]);
                            comm.ExecuteNonQuery();
                        }

                        // Now check for existing deltas that have been applied
                        comm.CommandText = "SELECT DeltaNumber FROM DatabaseVersionInformation WHERE ChangesetName = @changesetName ORDER BY DeltaNumber ASC";
                        comm.Parameters.AddWithValue("@changesetName", _argumentParser.ChangesetName);
                        using (SqlDataReader reader = comm.ExecuteReader())
                            while (reader.Read())
                            {
                                int delta = (int)reader[0];
                                alreadyApplied.Add(delta);
                                if (deltasToApply.Contains(delta))
                                {
                                    deltasToApply.Remove(delta);
                                    string correspondingFilename = deltasToApplyCorrespondingFilenames.First(df => df.StartsWith(delta + " "));
                                    deltasToApplyCorrespondingFilenames.Remove(correspondingFilename);
                                }
                            }
                        statusMessages.Add("Determined " + alreadyApplied.Count + " existing deltas");
                        Console.WriteLine(statusMessages[statusMessages.Count - 1]);
                        comm.Parameters.Clear();

                        // Check for any failed deltas on this changeset
                        comm.CommandText = "SELECT TOP 1 'Exists' FROM DatabaseVersionInformation WHERE ChangesetName = @changesetName AND ApplyCompleteTime IS NULL";
                        comm.Parameters.AddWithValue("@changesetName", _argumentParser.ChangesetName);
                        response = comm.ExecuteScalar();
                        if (response != null && response as string == "Exists")
                            return new DeltaApplicationResults(alreadyApplied.ToArray(), successful.ToArray(), statusMessages.ToArray(), 0, deltasToApply.ToArray(), "Database is in an inconsistent state with partially applied deltas for this changeset");
                        comm.Parameters.Clear();

                        // Now for each delta, let's apply them
                        foreach (int delta in deltasToApply)
                        {
                            // Get a handle to the delta file and read it in
                            string correspondingFilename = deltasToApplyCorrespondingFilenames.First(df => df.StartsWith(delta + " "));
                            lastDelta = delta;

                            // Add delta row saying we've started
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@deltaNumber", delta);
                            comm.Parameters.AddWithValue("@changesetName", _argumentParser.ChangesetName);
                            comm.Parameters.AddWithValue("@filename", correspondingFilename);
                            comm.CommandText = "INSERT INTO DatabaseVersionInformation (ChangesetName, DeltaNumber, Filename, ApplyStartTime) VALUES(@changesetName, @deltaNumber, @filename, GETUTCDATE())";
                            comm.ExecuteNonQuery();

                            // Apply the delta
                            statusMessages.Add("Applying delta " + delta);
                            Console.WriteLine(statusMessages[statusMessages.Count - 1]);
                            using (Process proc = new Process 
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "sqlcmd",
                                    Arguments = "-S \"" + _argumentParser.DatabaseServer + "\" -U \"" + _argumentParser.DatabaseUsername + "\" -P \"" + _argumentParser.DatabasePassword + "\" -d \"" + _argumentParser.DatabaseName + "\" -i \"" + Path.Combine(_argumentParser.DeltaPath, correspondingFilename) + "\"",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true
                                }
                            })
                            {
                                proc.Start();
                                StreamReader[] readers = new StreamReader[] { proc.StandardOutput, proc.StandardError };
                                while (true)
                                {
                                    foreach (StreamReader reader in readers)
                                        while (!reader.EndOfStream)
                                        {
                                            string line = reader.ReadLine();
                                            statusMessages.Add(line);
                                            Console.WriteLine(line);
                                        }
                                    if (proc.HasExited)
                                    {
                                        if (proc.ExitCode != 0)
                                            // Delta has failed to apply so throw an error
                                            throw new Exception("sqlcmd failed with error " + proc.ExitCode);
                                        break;
                                    }
                                }
                            }

                            // Add delta row saying we've finished
                            comm.Parameters.Clear();
                            comm.Parameters.AddWithValue("@deltaNumber", delta);
                            comm.Parameters.AddWithValue("@changesetName", _argumentParser.ChangesetName);
                            comm.CommandText = "UPDATE DatabaseVersionInformation SET ApplyCompleteTime = GETUTCDATE() WHERE ChangesetName = @changesetName AND DeltaNumber = @deltaNumber";
                            comm.ExecuteNonQuery();

                            // Mark this as successful
                            successful.Add(delta);
                        }

                        // Return a success
                        return new DeltaApplicationResults(alreadyApplied.ToArray(), successful.ToArray(), statusMessages.ToArray());

                    }
                }
            }
            catch (Exception ex)
            {
                return new DeltaApplicationResults(alreadyApplied.ToArray(), successful.ToArray(), statusMessages.ToArray(), lastDelta.HasValue ? lastDelta.Value : 0, deltasToApply.Where(d => !successful.Contains(d)).ToArray(), "Unhandled exception." + Environment.NewLine + ex);
            }
        }
    }
}