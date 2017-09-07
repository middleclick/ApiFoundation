namespace XaXd.Api
{
    /// <summary>
    /// Indicates the type of resources being delivered from the delivery group.
    /// </summary>
    public enum Delivering
    {
        /// <summary>
        /// Indicates the site is at a version higher than the API supports.
        /// </summary>
        Unknown,

        /// <summary>
        /// Nothing currently delivered from the delivery group.
        /// </summary>
        None,

        /// <summary>
        /// Apps are being delivered from the delivery group.
        /// </summary>
        Apps,

        /// <summary>
        /// Desktops are being delivered from the delivery group.
        /// </summary>
        Desktops,

        /// <summary>
        /// Apps and desktops are being delivered from the delivery group.
        /// </summary>
        AppsAndDesktops,
    }
}