namespace XaXd.Api
{
    /// <summary>
    /// The type of resources being published.
    /// </summary>
    public enum DeliveryKind
    {
        /// <summary>
        /// Indicates the site is at a version higher than the API supports.
        /// </summary>
        Unknown,
        /// <summary>
        /// Delivery group supports desktops only.
        /// </summary>
        DesktopsOnly,
        /// <summary>
        /// Delivery group supports applications only.
        /// </summary>
        AppsOnly,
        /// <summary>
        /// Delivery group supports desktops and applications.
        /// </summary>
        AppsAndDesktops,
    }
}