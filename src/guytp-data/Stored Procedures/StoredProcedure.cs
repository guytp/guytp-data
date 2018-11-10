using Guytp.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Guytp.Data
{
    /// <summary>
    /// This class represents a stored procedure and is used by the repository to call a SQL Server stored procedure.  It provides a base implementation of the required functionality.
    /// </summary>
    public abstract class StoredProcedure
    {
        #region Declarations
        /// <summary>
        /// Defines a locker object around the cache.
        /// </summary>
        private readonly static object CacheLocker = new object();

        /// <summary>
        /// Defines the logger to use in this object.
        /// </summary>
        private static readonly Logger Logger = Logger.ApplicationInstance;

        /// <summary>
        /// Defines a list of all the properties representing input parameters on all stored procedures that are cached to prevent unneccessary reflection.
        /// </summary>
        private static readonly Dictionary<Type, PropertyInfo[]> CachedInputParameterProperties = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Defines a list of all the properties representing output parameters on all stored procedures that are cached to prevent unneccessary reflection.
        /// </summary>
        private static readonly Dictionary<Type, PropertyInfo[]> CachedOutputParameterProperties = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Defines a list of all the properties representing parameters on all stored procedures that are cached to prevent unneccessary reflection.
        /// </summary>
        private static readonly Dictionary<Type, PropertyInfo[]> CachedAllParameterProperties = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Defines a cached list of all the constructors for dataset types to proevent unneccessary reflection.
        /// </summary>
        private static readonly Dictionary<Type, ConstructorInfo[]> CachedDataSetConstructors = new Dictionary<Type, ConstructorInfo[]>();

        /// <summary>
        /// Defines a cached list of a mapping between properties on a class and the corresponding parameter attributes.
        /// </summary>
        private static readonly Dictionary<Type, ReadOnlyDictionary<PropertyInfo, StoredProcedureParameterAttribute>> CachedPropertyAttributes = new Dictionary<Type, ReadOnlyDictionary<PropertyInfo, StoredProcedureParameterAttribute>>();

        /// <summary>
        /// Defines a list of all the properties on this class that are stored procedure input values.
        /// </summary>
        private readonly PropertyInfo[] _inputParameterProperties;

        /// <summary>
        /// Defines a list of all the properties on this class that are stored procedure output values.
        /// </summary>
        private readonly PropertyInfo[] _outputParameterProperties;

        /// <summary>
        /// Defines a list of all the properties on this class that are stored procedure output values.
        /// </summary>
        private readonly PropertyInfo[] _allParameterProperties;

        /// <summary>
        /// Defines the cache of constructors for the datasets used by this procedure.
        /// </summary>
        private readonly ConstructorInfo[] _dataSetConstructors;

        /// <summary>
        /// Defines an array containing the types of data returend in data set from this stored procedure.
        /// </summary>
        private readonly Type[] _dataSetTypes;

        /// <summary>
        /// Defines a map between properties on this class and their corresponding stored procedure parameter attributes.
        /// </summary>
        private readonly ReadOnlyDictionary<PropertyInfo, StoredProcedureParameterAttribute> _propertyAttributeMap;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the stored procedure.
        /// </summary>
        public string ProcedureName { get; }

        /// <summary>
        /// Gets the optional timeout for this stored procedure.
        /// </summary>
        public int? Timeout { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="procedureName">
        /// The name of the stored procedure.
        /// </param>
        /// <param name="dataSetTypes">
        /// An array containing the types of data returend in data set from this stored procedure.
        /// </param>
        /// <param name="useReflection">
        /// Whether or not to use reflection (defaulting to true).  If this is turned off
        /// </param>
        /// <param name="timeout">
        /// The optional timeout for this stored procedure.
        /// </param>
        protected StoredProcedure(string procedureName, Type[] dataSetTypes = null, bool useReflection = true, int? timeout = null)
        {
            // Store our timeout
            Timeout = timeout;

            // Populate the reflection cache if not already populated
            if (useReflection)
            {
                Type thisType = GetType();
                bool isCached;
                lock (CacheLocker)
                {
                    isCached = CachedInputParameterProperties.ContainsKey(thisType);
                    if (!isCached)
                    {
                        // Get input and output properties
                        List<PropertyInfo> inputParameterProperties = new List<PropertyInfo>();
                        List<PropertyInfo> outputParameterProperties = new List<PropertyInfo>();
                        List<PropertyInfo> allParameterProperties = new List<PropertyInfo>();
                        Dictionary<PropertyInfo, StoredProcedureParameterAttribute> propertyAttributeMap = new Dictionary<PropertyInfo, StoredProcedureParameterAttribute>();
                        PropertyInfo[] allProperties = thisType.GetProperties();
                        foreach (PropertyInfo property in allProperties)
                        {
                            StoredProcedureParameterAttribute attribute = property.GetCustomAttribute<StoredProcedureParameterAttribute>();
                            if (attribute == null)
                                continue;
                            if (attribute.Direction == ParameterDirection.Input || attribute.Direction == ParameterDirection.InputOutput)
                                inputParameterProperties.Add(property);
                            if (attribute.Direction == ParameterDirection.Output || attribute.Direction == ParameterDirection.InputOutput)
                                outputParameterProperties.Add(property);
                            allParameterProperties.Add(property);
                            propertyAttributeMap.Add(property, attribute);
                        }

                        // Now get constructors to use for dataset types
                        List<ConstructorInfo> constructors = new List<ConstructorInfo>();
                        if (dataSetTypes != null)
                            foreach (Type dataSetType in dataSetTypes)
                                // Add the constructor here even if null - a descendent type may expect it
                                constructors.Add(dataSetType.GetConstructors().FirstOrDefault(ci => ci.GetCustomAttribute<StoredProcedureDataSetConstructorAttribute>() != null));

                        // Store these at class level
                        _inputParameterProperties = inputParameterProperties.ToArray();
                        _outputParameterProperties = outputParameterProperties.ToArray();
                        _allParameterProperties = allParameterProperties.ToArray();
                        _dataSetConstructors = constructors.ToArray();
                        _propertyAttributeMap = new ReadOnlyDictionary<PropertyInfo, StoredProcedureParameterAttribute>(propertyAttributeMap);

                        // Store these in the global cache
                        CachedAllParameterProperties.Add(thisType, _allParameterProperties);
                        CachedInputParameterProperties.Add(thisType, _inputParameterProperties);
                        CachedOutputParameterProperties.Add(thisType, _outputParameterProperties);
                        CachedDataSetConstructors.Add(thisType, _dataSetConstructors);
                        CachedPropertyAttributes.Add(thisType, _propertyAttributeMap);
                    }
                    else
                    {
                        _inputParameterProperties = CachedInputParameterProperties[thisType];
                        _outputParameterProperties = CachedOutputParameterProperties[thisType];
                        _allParameterProperties = CachedAllParameterProperties[thisType];
                        _dataSetConstructors = CachedDataSetConstructors[thisType];
                        _propertyAttributeMap = CachedPropertyAttributes[thisType];
                    }
                }
            }

            // Store our data set types
            _dataSetTypes = dataSetTypes;

            // Store values
            ProcedureName = procedureName;
        }
        #endregion

        #region Default Implementations
        /// <summary>
        /// Creates data parameters for the stored procedure based off this object.
        /// </summary>
        /// <returns></returns>
        public virtual IDataParameter[] GenerateDataParameters()
        {
            Logger.Trace("Generating data parameters");
            List<IDataParameter> parameters = new List<IDataParameter>();
            foreach (PropertyInfo property in _allParameterProperties)
            {
                Logger.Trace("Processing property " + property.Name);
                bool isInput = _inputParameterProperties.Contains(property);
                bool isOutput = _outputParameterProperties.Contains(property);
                SqlParameter parameter = new SqlParameter();
                parameter.ParameterName = "@" + property.Name.ToLower()[0] + (property.Name.Length > 1 ? property.Name.Substring(1) : string.Empty);
                parameter.Value = isInput ? property.GetValue(this) ?? DBNull.Value : DBNull.Value;
                if (_propertyAttributeMap != null && _propertyAttributeMap.ContainsKey(property))
                {
                    StoredProcedureParameterAttribute attribute = _propertyAttributeMap[property];
                    if (attribute != null)
                    {
                        if (attribute.Size.HasValue)
                            parameter.Size = attribute.Size.Value;
                        if (!string.IsNullOrEmpty(attribute.TypeName))
                            parameter.TypeName = attribute.TypeName;
                        if (attribute.Scale.HasValue)
                            parameter.Scale = attribute.Scale.Value;
                        if (attribute.Precision.HasValue)
                            parameter.Precision = attribute.Precision.Value;
                    }
                }
                parameter.Direction = isInput && isOutput ? ParameterDirection.InputOutput : isInput ? ParameterDirection.Input : ParameterDirection.Output;
                parameter.SqlDbType = property.PropertyType == typeof(DataTable) ? SqlDbType.Structured : SqlHelper.GetDbType(property.PropertyType);
                Logger.Trace("Set " + parameter.ParameterName + " to " + parameter.Value + "(" + parameter.Direction + ") of type " + parameter.SqlDbType);
                parameters.Add(parameter);
            }
            return parameters.ToArray(); ;
        }

        /// <summary>
        /// Parse a list of supplied data parameters extract any return values or output parameters and setting the corresponding properties on this object.
        /// </summary>
        /// <param name="parameters">
        /// The parameters to parse.
        /// </param>
        public virtual void ParseDataParametersForOutput(IDataParameter[] parameters)
        {
            // Find all of the parameters that are output parameters and set corresponding propeties with their values
            Logger.Trace("Processing all output parameters from query");
            foreach (IDataParameter parameter in parameters)
            {
                string propertyName = parameter.ParameterName.Substring(1, 1).ToUpper() + (parameter.ParameterName.Length > 2 ? parameter.ParameterName.Substring(2) : string.Empty);
                PropertyInfo property = _outputParameterProperties.FirstOrDefault(p => p.Name == propertyName);
                if (property == null)
                    continue;
                Logger.Trace("Setting " + propertyName + " property after output of " + ProcedureName);
                property.SetValue(this, parameter.Value == DBNull.Value ? null : parameter.Value);
            }
        }

        /// <summary>
        /// Parses a single row of data from the result of the stored procedure.
        /// </summary>
        /// <param name="dataRecord">
        /// The single row data record to parse.
        /// </param>
        /// <param name="dataSetNumber">
        /// Which data set, if this stored procedure can return more than one, is being parsed.  The first data set is numbered zero.
        /// </param>
        /// <returns>
        /// An object of a type relevent to the data set.
        /// </returns>
        public virtual object ParseRow(IDataRecord dataRecord, int dataSetNumber)
        {
            // Validate arguments
            if (dataSetNumber < 0 || _dataSetTypes.Length == 0 || dataSetNumber >= _dataSetTypes.Length)
                throw new ArgumentOutOfRangeException(nameof(dataSetNumber));

            // Get a handle on the type we are parsing
            Type dataSetType = _dataSetTypes[dataSetNumber];
            Logger.Trace("Parsing a row for dataset " + dataSetNumber + " (" + dataSetType.Name + ")");

            // Create a new object of this type
            ConstructorInfo constructor = _dataSetConstructors[dataSetNumber];
            if (constructor == null)
            {
                Logger.Error("Could not find suitable constructor on " + dataSetType + " to use for automatic parsing");
                throw new NotImplementedException("Unable to parse object in to type of " + dataSetType);
            }
            object[] parameters = new object[dataRecord.FieldCount];
            for (int i = 0; i < parameters.Length; i++)
            {
                object value = dataRecord.GetValue(i);
                if (value == DBNull.Value)
                    value = null;
                parameters[i] = value;
            }
            Logger.Trace("Creating a new instance of " + dataSetType.Name);
            return constructor.Invoke(parameters);
        }
        #endregion
    }
}