using System;

namespace ApiFoundation.Shared
{
    /// <summary>
    /// Mark a property or route as deprecated.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class DeprecatedAttribute : Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DeprecatedAttribute(string note = null) => Note = note;

        /// <summary>
        /// Deprecation note, for example to inform callers about alternatives.
        /// </summary>
        public string Note { get; }
    }
}