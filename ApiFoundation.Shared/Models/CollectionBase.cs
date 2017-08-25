using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiFoundation.Shared.Models
{
    public class CollectionBase<T> : LinkedResponse
    {
        public CollectionBase()
        {
            Items = new List<T>();
        }

        public IList<T> Items { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ContinuationToken { get; set; }
    }
}