using System.ComponentModel.DataAnnotations;
using ApiFoundation.Shared;
using ApiFoundation.Shared.Models;
using Newtonsoft.Json;

namespace XaXd.Api
{
    /// <summary>
    /// A delivery group object represents a collection of machines that are fully configured in a site that is able to run either a Microsoft Windows desktop environment, individual applications, or both.
    /// </summary>
    public class DeliveryGroup : LinkedResponse
    {
        /// <summary>
        /// Customer that owns the delivery group.
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// Id of the delivery site in which the delivery group is contained.
        /// </summary>
        /// <returns></returns>
        public string SiteId { get; set; }

        /// <summary>
        /// Id of a delivery group; universally unique even across customers.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique (but not _universally_ unique) identifier. May be the same in multiple sites.
        /// </summary>
        [Deprecated("Use Id property instead.")]
        public string Uid { get; set; }

        /// <summary>
        /// Universally unique identifier.
        /// </summary>
        [Deprecated("Use Id property instead.")]
        public string Uuid { get; set; }

        /// <summary>
        /// Name of the delivery group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If the delivery group is in maintenance mode, the machines within are temporarily
        /// suspended from launching new sessions.
        /// </summary>
        public bool InMaintenanceMode { get; set; }

        /// <summary>
        /// The type of resources being published.
        /// </summary>
        [Deprecated("DeliveryKind is now automatically set to AppsAndDesktops whenever necessary.")]
        public DeliveryKind DeliveryKind { get; set; }

        /// <summary>
        /// The kind of desktops published from the delivery group.
        /// </summary>
        public DesktopKind DesktopKind { get; set; }

        /// <summary>
        /// Number of desktops in a "Faulted" state.
        /// TBD: what does that mean?
        /// </summary>
        public int DesktopsFaulted { get; set; }

        /// <summary>
        /// The minimum functional level required for machines in the
        /// delivery group to be able to register with the site.
        /// </summary>
        /// <remarks>
        /// Increasing the minimum functional level enables newer features
        /// to operate within the delivery group.
        /// </remarks>
        public FunctionalLevel MinimumFunctionalLevel { get; set; }

        /// <summary>
        /// Total number of applications associated with the delivery group.
        /// </summary>
        public int TotalApplications { get; set; }

        /// <summary>
        /// Total number of machines in the delivery group.
        /// </summary>
        public int TotalDesktops { get; set; }

        /// <summary>
        /// The number of machines in the delivery group in state Available; this is the number of machines with no sessions present.
        /// </summary>
        public int DesktopsAvailable { get; set; }

        /// <summary>
        /// The total number of user sessions currently running on all of the machines in the delivery group.
        /// </summary>
        public int SessionCount { get; set; }

        /// <summary>
        /// The number of machines in the delivery group that are currently unregistered.
        /// </summary>
        public int DesktopsUnregistered { get; set; }

        /// <summary>
        /// The number of disconnected sessions present on machines in the delivery group.
        /// </summary>
        public int DesktopsDisconnected { get; set; }

        /// <summary>
        /// Specifies the session support (single/multi) of the machines in the delivery group. Machines with the incorrect session support for the delivery group will be unable to register with the Citrix Broker Service.
        /// </summary>
        public SessionSupport SessionSupport { get; set; }

        /// <summary>
        /// Specifies whether the delivery group is a Remote PC delivery group.
        /// </summary>
        /// <returns></returns>
        public bool IsRemotePC { get; set; }

        /// <summary>
        /// Indicates whether the delivery group has been promoted from a previous functional level.
        /// </summary>
        [Deprecated("Use the presence of HasBeenPromotedFrom property to determine this.")]
        public bool HasBeenPromoted { get; set; }

        /// <summary>
        /// If set, the delivery group has been promoted from a previous functional level.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FunctionalLevel? HasBeenPromotedFrom { get; set; }

        /// <summary>
        /// Indicates the type of resources being delivered from the delivery group.
        /// </summary>
        public Delivering Delivering { get; set; }

        /// <summary>
        /// Indicates the state of AppDNA compatibility analysis of the delivery group.
        /// </summary>
        public AppDNAState AppDNAState { get; set; }
    }
}