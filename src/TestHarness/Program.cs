using System;

namespace Guytp.Data.TestHarness
{
    /// <summary>
    /// This class is the main entry point to the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// This method is the main entry point to the application.
        /// </summary>
        /// <param name="args">
        /// An array of command line arguments.
        /// </param>
        public static void Main(string[] args)
        {
            using (TestRepository repository = new TestRepository())
            {
                CompositeResultObject objectOne = repository.GetCompositeResult("abc");
                CompositeResultObject objectTwo = repository.GetCompositeResult(null);
                Guid guidOne = repository.GetTestOutputGuidFromScalar();
                Guid guidTwo = repository.GetTestOutputGuidFromNonQuery();
                Console.Write("Database queried, break here and check data then press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}