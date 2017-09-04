using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiFoundation.Shared.Models
{
    /// <summary>
    /// A response containing a list of related links that are currently accessible by the caller.
    /// </summary>
    public class LinkedResponse
    {
        /// <summary>
        /// List of related links that are currently accessible by the caller.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "_links")]
        public virtual IList<Link> Links { get; set; }
    }

    /// <summary>
    /// A link, or related reference.
    /// </summary>
    /// <remarks>
    /// Note that this class is immutable.
    /// </remarks>
    public class Link
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Machine-readable name of the link.</param>
        /// <param name="href">Location where the caller can access the link.</param>
        /// <param name="method">HTTP verb used to access the link.  If not specified, GET is assumed.</param>
        public Link(string name, string href, string method = null)
        {
            Name = name;
            Href = href;
            Method = method;
        }

        /// <summary>
        /// Make a copy of a link but with a different href.
        /// </summary>
        public Link WithHref(Func<string, string> mapHref)
        {
            return new Link(Name, mapHref(Href), Method);
        }

        /// <summary>
        /// Machine-readable name of the link.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; private set; }

        /// <summary>
        /// Location where the caller can access the link.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Href { get; private set; }
        
        /// <summary>
        /// HTTP verb used to access the link.  If not specified, GET is assumed.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; private set; }
    }
}