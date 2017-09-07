namespace XaXd.Api
{
    /// <summary>
    /// The functional level required for machines in the
    /// delivery group to be able to register with the site.
    /// </summary>
    public enum FunctionalLevel
    {
        /// <summary>
        /// Indicates the site is at a version higher than the API supports.
        /// </summary>
        Unknown,

        /// <summary>
        /// XenDesktop 5+ compatible.
        /// </summary>
        L5,

        /// <summary>
        /// Support the minimum level (i.e. maximum version compatibility).
        /// Note that this implies all newer features are disabled.
        /// </summary>
        LMIN,

        /// <summary>
        /// XenDesktop 7.0+ compatible.
        /// </summary>
        L7,

        /// <summary>
        /// XenDesktop 7.6+ compatible.
        /// </summary>
        L7_6,

        /// <summary>
        /// XenDesktop 7.7+ compatible.
        /// </summary>
        L7_7,

        /// <summary>
        /// XenDesktop 7.8+ compatible.
        /// </summary>
        L7_8,

        /// <summary>
        /// XenDesktop 7.9+ compatible.
        /// </summary>
        L7_9,

        /// <summary>
        /// Enable all delivery group features.
        /// Note that this requires all machines in the delivery group
        /// to have the newest version of the delivery agent.
        /// </summary>
        LMAX
    }
}