using System;
using System.Collections.Generic;
using System.Data;

namespace Guytp.Data
{
    /// <summary>
    /// This class provides core helper methods for SQL Server integration.
    /// </summary>
    internal static class SqlHelper
    {
        #region Declarations
        /// <summary>
        /// Defines a mapping between .Net and SQL Server database types.
        /// </summary>
        private static readonly Dictionary<Type, SqlDbType> NetToSqlTypeMap;
        #endregion

        #region Constructors
        /// <summary>
        /// Perform one-time initialisation for the class.
        /// </summary>
        static SqlHelper()
        {
            NetToSqlTypeMap = new Dictionary<Type, SqlDbType>();
            NetToSqlTypeMap[typeof(string)] = SqlDbType.NVarChar;
            NetToSqlTypeMap[typeof(byte[])] = SqlDbType.Image;
            NetToSqlTypeMap[typeof(char[])] = SqlDbType.NVarChar;
            NetToSqlTypeMap[typeof(byte)] = SqlDbType.TinyInt;
            NetToSqlTypeMap[typeof(short)] = SqlDbType.SmallInt;
            NetToSqlTypeMap[typeof(int)] = SqlDbType.Int;
            NetToSqlTypeMap[typeof(long)] = SqlDbType.BigInt;
            NetToSqlTypeMap[typeof(bool)] = SqlDbType.Bit;
            NetToSqlTypeMap[typeof(decimal)] = SqlDbType.Decimal;
            NetToSqlTypeMap[typeof(float)] = SqlDbType.Real;
            NetToSqlTypeMap[typeof(double)] = SqlDbType.Float;
            NetToSqlTypeMap[typeof(DateTime)] = SqlDbType.DateTime2;
            NetToSqlTypeMap[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset;
            NetToSqlTypeMap[typeof(TimeSpan)] = SqlDbType.Time;
            NetToSqlTypeMap[typeof(Guid)] = SqlDbType.UniqueIdentifier;
        }
        #endregion

        /// <summary>
        /// Convert a .Net type to a corresponding SQL database type.
        /// </summary>
        /// <param name="dotNetType">
        /// The .Net type to get a SQL database type for.
        /// </param>
        /// <returns>
        /// A SqlDbType if known, otherwise an ArgumentException is thrown.
        /// </returns>
        public static SqlDbType GetDbType(Type dotNetType)
        {
            Type typeToCheck = dotNetType;
            if (dotNetType.IsGenericType && dotNetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                typeToCheck = dotNetType.GetGenericArguments()[0];
            if (typeToCheck.IsEnum)
                typeToCheck = Enum.GetUnderlyingType(typeToCheck);
            if (NetToSqlTypeMap.ContainsKey(typeToCheck))
                return NetToSqlTypeMap[typeToCheck];
            throw new ArgumentException($"{dotNetType.FullName} is not a supported .NET class");
        }


        /// <summary>
        /// Convert a .Net type to a corresponding SQL database type.
        /// </summary>
        /// <typeparam name="DotNetType">
        /// The .Net type to get a SQL database type for.
        /// </typeparam>
        /// <returns>
        /// A SqlDbType if known, otherwise an ArgumentException is thrown.
        /// </returns>
        /// <returns></returns>
        public static SqlDbType GetDbType<DotNetType>()
        {
            return GetDbType(typeof(DotNetType));
        }
    }
}