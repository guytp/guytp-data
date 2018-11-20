using System.IO;
using System.Linq;

namespace Guytp.Data.DatabaseDeltaApplicator
{
    /// <summary>
    /// This class parses the command line arguments supplied to the application.
    /// </summary>
    internal class ArgumentParser
    {
        #region Properties
        /// <summary>
        /// Gets the name of the database that the applicator runs against.
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// Gets the name of the server that the applicator runs against.
        /// </summary>
        public string DatabaseServer { get; }

        public string DatabaseUsername { get; }

        public string DatabasePassword { get; }

        /// <summary>
        /// Gets the name of changeset being applied.
        /// </summary>
        public string ChangesetName { get; }

        /// <summary>
        /// Gets the path to the delta files.
        /// </summary>
        public string DeltaPath { get; }

        /// <summary>
        /// Gets whether or not the application arguments are valid.
        /// </summary>
        public bool IsValid { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="args">
        /// The command line arguments passed to the application.
        /// </param>
        public ArgumentParser(string[] args)
        {
            // Validate parameters
            if (args == null || args.Length != 6 || args.Any(a => string.IsNullOrEmpty(a)))
                return;

            // Store the values
            DatabaseServer = args[0];
            DatabaseUsername = args[1];
            DatabasePassword = args[2];
            DatabaseName = args[3];
            ChangesetName = args[4];
            DeltaPath = args[5];
            if (!Directory.Exists(DeltaPath))
            {
                IsValid = false;
                return;
            }
            IsValid = true;
        }
        #endregion

        /// <summary>
        /// Displays a help message for how to use the application.
        /// </summary>
        /// <returns>
        /// The help message to display to the user.
        /// </returns>
        public string GetHelpMessage()
        {
            return "Usage: DatabaseDeltaApplicator <Database Server> <Username> <Password> <Database Name> <Changeset Name> <Path>";
        }
    }
}