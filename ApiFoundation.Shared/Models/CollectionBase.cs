using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiFoundation.Shared.Models
{
    /// <summary>
    /// Base class of item collection responses.
    /// </summary>
    public class CollectionBase<T> : LinkedResponse
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CollectionBase()
        {
            Items = new List<T>();
        }

        /// <summary>
        /// List of response items.
        /// </summary>
        public IList<T> Items { get; set; }

        /// <summary>
        /// Continuation token.
        /// </summary>
        /// <remarks>
        /// If present, indicates that the response is incomplete; the caller
        /// should call again with the continuation token if more results are
        /// desired.
        /// </remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ContinuationToken { get; set; }
    }

    /// <summary>
    /// Interface with covariant item collection access.
    /// </summary>
    public interface ILinkedCollectionBase<out T>
    {
        /// <summary>
        /// Get the items in a covariant type compatible way.
        /// </summary>
        IEnumerable<T> GetItems();
    }

    /// <summary>
    /// Base class of a linked collection of items, where each item in the collection also has links.
    /// </summary>
    public class LinkedCollectionBase<T> : CollectionBase<T>, ILinkedCollectionBase<T> where T : LinkedResponse
    {
        /// <summary>
        /// Get the items in a covariant type compatible way.
        /// </summary>
        public IEnumerable<T> GetItems()
        {
            return Items;
        }
    }
}