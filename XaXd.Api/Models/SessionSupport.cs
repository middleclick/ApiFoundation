namespace XaXd.Api
{
    /// <summary>
    /// Specifies the session support (single/multi) of the machines in the delivery group.
    /// </summary>
    public enum SessionSupport
    {
        /// <summary>
        /// Indicates the site is at a version higher than the API supports.
        /// </summary>
        Unknown,

        /// <summary>
        /// Delivery group supports single-session machines.
        /// </summary>
        SingleSession,

        /// <summary>
        /// Delivery group supports multi-session machines.
        /// </summary>
        MultiSession,
    }
}