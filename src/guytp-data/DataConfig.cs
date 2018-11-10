using Guytp.Config;

namespace Guytp.Data
{
    /// <summary>
    /// This class provides the application SQL database configuration.
    /// </summary>
    public class DataConfig
    {
        #region Declarations
        /// <summary>
        /// Defines the configuraion file we are using.
        /// </summary>
        private readonly AppConfig _config;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the configuration for the whole application.
        /// </summary>
        public static DataConfig ApplicationInstance { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Handle one time initialisation of this class.
        /// </summary>
        static DataConfig()
        {
            ApplicationInstance = new DataConfig(AppConfig.ApplicationInstance);
        }

        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="config">
        /// The configuration file we are using.
        /// </param>
        private DataConfig(AppConfig config)
        {
            _config = config;
        }
        #endregion

        /// <summary>
        /// Gets the connection string for the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the connection string.
        /// </param>
        /// <returns>
        /// The connection string.
        /// </returns>
        public string GetConnectionString(string name)
        {
            return _config.GetConnectionString(name);
        }
    }
}