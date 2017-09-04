using System.Collections.Generic;
using ApiFoundation.Shared.Rbac;

namespace ApiFoundation.Rbac
{
    internal class CcAuthValidationContext
    {
        public IList<CcAuthRequiredPolicy> RequiredPolicies { get; set; }
        public IDictionary<string, string> ScopeParameters { get; set; }
    }
}