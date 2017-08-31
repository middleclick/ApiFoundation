using System;

namespace ApiFoundation.Rbac
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RbacPermissionAttribute : Attribute
    {
        public RbacPermissionAttribute(params string[] permissions) => Permissions = permissions;

        public string[] Permissions { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class RbacScopeAttribute : Attribute
    {
        public RbacScopeAttribute(string scopeType) => ScopeType = scopeType;

        public string ScopeType { get; set; }
    }
}