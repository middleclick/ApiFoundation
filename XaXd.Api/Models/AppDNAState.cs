namespace XaXd.Api
{
    /// <summary>
    /// Indicates the state of AppDNA compatibility analysis of the delivery group.
    /// </summary>
    public enum AppDNAState
    {
        /// <summary>
        /// Indicates the site is at a version higher than the API supports.
        /// </summary>
        Unknown,

        /// <summary>
        /// The delivery group does not support AppDNA.
        /// </summary>
        Unsupported,

        /// <summary>
        /// AppDNA is importing compatibility data for the delivery group.
        /// </summary>
        Importing,

        /// <summary>
        /// AppDNA is capturing compatibility data for the delivery group.
        /// </summary>
        Capturing,

        /// <summary>
        /// AppDNA is analyzing compatibility data for the delivery group.
        /// </summary>
        Analyzing,
        
        /// <summary>
        /// AppDNA was unsuccessful in analyzing compatibility data for the delivery group.
        /// </summary>
        Error,

        /// <summary>
        /// AppDNA determined that the delivery group is compatible.
        /// </summary>
        Compatible,

        /// <summary>
        /// AppDNA detected problems with the delivery group compatibility.
        /// </summary>
        ProblemsDetected,
    }
}