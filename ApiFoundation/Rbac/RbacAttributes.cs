using System;

namespace ApiFoundation.Rbac
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RbacPermissionAttribute : Attribute
    {
        public RbacPermissionAttribute(params string[] permissions) => Permissions = permissions;

        public string[] Permissions { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RbacScopeAttribute : Attribute
    {
        public RbacScopeAttribute(params string[] scopes) => Scopes = scopes;

        public string[] Scopes { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class RbacScopeParamAttribute : Attribute
    {
        public RbacScopeParamAttribute(string scopeParam) => ScopeParam = scopeParam;

        public string ScopeParam { get; set; }
    }
}