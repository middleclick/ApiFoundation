using System;

namespace ApiFoundation.Shared
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiVersionAttribute : Attribute
    {
        public ApiVersionAttribute(string introductionDate) => IntroductionDate = introductionDate;

        public string IntroductionDate { get; }
    }
}