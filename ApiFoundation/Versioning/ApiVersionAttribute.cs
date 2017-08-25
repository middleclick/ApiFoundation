using System;

namespace ApiFoundation.Versioning
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiVersionAttribute : Attribute
    {
        internal ApiVersionAttribute(string introductionDate) => IntroductionDate = introductionDate;

        public string IntroductionDate { get; }
    }
}