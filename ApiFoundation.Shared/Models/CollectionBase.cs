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

    public interface ILinkedCollectionBase<out T>
    {
        IEnumerable<T> GetItems();
    }

    public class LinkedCollectionBase<T> : CollectionBase<T>, ILinkedCollectionBase<T> where T : LinkedResponse
    {
        public IEnumerable<T> GetItems()
        {
            return Items;
        }
    }
}