using System;

namespace Guytp.Data.DatabaseDeltaApplicator
{
    /// <summary>
    /// This class provides the main entry point for the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// This method provides the main entry point for the application.
        /// </summary>
        /// <param name="args">
        /// The arguments for the application.
        /// </param>
        public static int Main(string[] args)
        {
            // Ensure arguments are valid
            ArgumentParser argumentParser = new ArgumentParser(args);
            if (!argumentParser.IsValid)
            {
                Console.WriteLine(argumentParser.GetHelpMessage());
                return 1;
            }

            try
            {
                // Apply deltas
                DeltaApplicator applicator = new DeltaApplicator(argumentParser);
                DeltaApplicationResults results = applicator.Apply();
                Console.WriteLine(results.ToString());
                if (!results.IsSuccess)
                    return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal: Unhandled exception.\r\n" + ex);
                return 2;
            }

            // Return a success
            return 0;
        }
    }
}