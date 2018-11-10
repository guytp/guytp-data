using System;

namespace Guytp.Data
{
    /// <summary>
    /// This attribute is used to identify the constructor that should be called within a StoredProcedure's dataset.  It must be supplied exactly once.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class StoredProcedureDataSetConstructorAttribute : Attribute
    {
    }
}