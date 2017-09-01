using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiFoundation.Shared.Models
{
    public class LinkedResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "_links")]
        public virtual IList<Link> Links { get; set; }
    }

    public class Link
    {
        public Link(string name, string href, string method = null)
        {
            Name = name;
            Href = href;
            Method = method;
        }

        public Link Duplicate()
        {
            return new Link(Name, Href, Method);
        }

        public Link WithHref(Func<string, string> mapHref)
        {
            return new Link(Name, mapHref(Href), Method);
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Href { get; private set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; private set; }
    }
}