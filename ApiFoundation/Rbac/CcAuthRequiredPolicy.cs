using System.Collections.Generic;
using System.Security.Principal;

namespace ApiFoundation.Rbac
{
    internal class CcAuthRequiredPolicy
    {
        public IList<string> Policies { get; set; }
        public IList<string> ScopeTemplates { get; set; }
    }
}