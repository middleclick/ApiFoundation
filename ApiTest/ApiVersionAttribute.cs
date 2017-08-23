using System;

namespace ApiTest
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ApiVersionAttribute : Attribute
    {
        internal ApiVersionAttribute(string introductionDate) => IntroductionDate = introductionDate;

        public string IntroductionDate { get; }
    }
}