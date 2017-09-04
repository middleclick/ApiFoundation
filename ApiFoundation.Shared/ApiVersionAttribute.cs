using System;

namespace ApiFoundation.Shared
{
    /// <summary>
    /// Apply this attribute to an API to indicate its introduction date in the documentation.
    /// </summary>    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiVersionAttribute : Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="introductionDate">Date when the API was introduced, in YYYY-MM-DD format.</param>
        public ApiVersionAttribute(string introductionDate) => IntroductionDate = introductionDate;

        /// <summary>
        /// Introduction date of the API, in YYYY-MM-DD format.
        /// </summary>
        public string IntroductionDate { get; }
    }
}