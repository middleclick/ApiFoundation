using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiFoundation.ResourceLinking
{
    public class LinkedResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "_links")]
        public virtual IList<Link> Links { get; set; }
    }

    public class Link
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Href { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; set; }
    }
}