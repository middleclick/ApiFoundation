using System;

namespace ApiFoundation.Shared.Rbac
{
    /// <summary>
    /// Permissions required to call a method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RbacPermissionAttribute : Attribute
    {
        /// <summary>
        /// Permissions required to call a method.
        /// </summary>
        /// <remarks>
        /// The list of required permissions is AND-ed together.  In future, OR-ed
        /// permissions may be allowed by specifying multiple attributes on the same
        /// method.
        /// </remarks>
        /// <example><pre>[RbacPermission("CC:Admin:XenDesktop:GetDeliveryGroups")]</pre></example>
        public RbacPermissionAttribute(params string[] permissions) => Permissions = permissions;

        /// <summary>
        /// List of permissions required by the caller.
        /// </summary>
        public string[] Permissions { get; private set; }
    }

    /// <summary>
    /// Scope pattern required to call a method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RbacScopeAttribute : Attribute
    {
        /// <summary>
        /// Scope patterns required to call a method.
        /// </summary>
        /// <remarks>
        /// The scope pattern should have templated parameters that get filled in
        /// from the route parameters which have the <see cref="RbacScopeParamAttribute" />.
        /// The list of scope patterns is AND-ed together.
        /// In future, OR-ed scope patterns may be allowed by specifying multiple
        /// attributes on the same method.  (This would require named permission sets.)
        /// </remarks>
        /// <example><pre>[RbacScope("CC:c_[customer]:XenDesktop:ANY:ANY")]</pre></example>
        public RbacScopeAttribute(params string[] scopes) => Scopes = scopes;

        /// <summary>
        /// List of scope patterns required to call a method.
        /// </summary>
        public string[] Scopes { get; private set; }
    }

    /// <summary>
    /// Parameter that gets filled into a scope pattern.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RbacScopeParamAttribute : Attribute
    {
        /// <summary>
        /// Parameter that gets filled into a scope pattern.
        /// </summary>
        /// <remarks>
        /// Apply this attribute to a method parameter and it will connect
        /// that method parameter to the named scope template parameter.
        /// </remarks>
        public RbacScopeParamAttribute(string scopeParam) => ScopeParam = scopeParam;

        /// <summary>
        /// Scope parameter name to connect the method parameter to.
        /// </summary>
        public string ScopeParam { get; set; }
    }
}