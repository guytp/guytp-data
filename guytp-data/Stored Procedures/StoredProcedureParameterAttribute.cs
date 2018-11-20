using System;
using System.Data;

namespace Guytp.Data
{
    /// <summary>
    /// This attribute is used to define what kind of property a StoredProcedure object is in related to its direction.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class StoredProcedureParameterAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Gets the direction of the parameter.
        /// </summary>
        public ParameterDirection Direction { get; }

        /// <summary>
        /// Gets the optional size of the parameter.
        /// </summary>
        public int? Size { get; }

        /// <summary>
        /// Gets the SQL Server TVP type name, when using data tables
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the optional precision for the type where appropriate (i.e. with decimals).
        /// </summary>
        public byte? Precision { get; }

        /// <summary>
        /// Gets the optional scale for the type where appropriate (i.e. with decimals).
        /// </summary>
        public byte? Scale { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="direction">
        /// The direction of the parameter.
        /// </param>
        /// <param name="typeName">
        /// The SQL Server TVP type name to use, when using data tables.
        /// </param>
        public StoredProcedureParameterAttribute(ParameterDirection direction = ParameterDirection.Input, string typeName = null)
        {
            Direction = direction;
            TypeName = typeName;
        }

        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="direction">
        /// The direction of the parameter.
        /// </param>
        /// <param name="size">
        /// The size of the parameter.
        /// </param>
        public StoredProcedureParameterAttribute(ParameterDirection direction, int size)
        {
            Direction = direction;
            Size = size;
        }

        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="direction">
        /// The direction of the parameter.
        /// </param>
        /// <param name="precision">
        /// The optional precision for the type where appropriate (i.e. with decimals).
        /// </param>
        /// <param name="scale">
        /// The optional scale for the type where appropriate (i.e. with decimals).
        /// </param>
        public StoredProcedureParameterAttribute(ParameterDirection direction, byte precision, byte scale)
        {
            Direction = direction;
            Precision = precision;
            Scale = scale;
        }
        #endregion
    }
}