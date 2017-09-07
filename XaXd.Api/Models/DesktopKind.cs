namespace XaXd.Api
{
    /// <summary>
    /// The kind of desktops published from the delivery group.
    /// </summary>
    public enum DesktopKind
    {
        /// <summary>
        /// Indicates the site is at a version higher than the API supports.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Desktops in the delivery group are private.
        /// </summary>
        /// <remarks>
        /// This does not mean that _all_ desktops in the delivery group
        /// are private; only that, by default, desktops in the delivery
        /// group are private.
        /// </remarks>
        Private,
        
        /// <summary>
        /// All desktops in the delivery group are shared.
        /// </summary>
        /// <remarks>
        /// This does not mean that _all_ desktops in the delivery group
        /// are shared; only that, by default, desktops in the delivery
        /// group are shared.
        /// </remarks>
        Shared,
    }
}