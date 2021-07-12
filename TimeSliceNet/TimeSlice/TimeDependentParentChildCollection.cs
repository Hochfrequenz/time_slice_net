using System.Collections;
using System.Text.Json.Serialization;

namespace TimeSlice
{

    /// <summary>
    /// Describes the possible kinds of time dependent relationships between parents and childs
    /// </summary>
    public enum TimeDependentCollectionType
    {
        /// <summary>
        /// &lt;= 1 child assigned to a parent at a time
        /// </summary>
        [JsonPropertyName("allowOverlaps")]
        AllowOverlaps,
        /// <summary>
        /// prevent overlapping time slices (there might be &gt;1 child assigned to a parent at a time
        /// </summary>
        [JsonPropertyName("preventOverlaps")]
        PreventOverlaps
    }
    
    /// <summary>
    /// the simplest way to describe a time dependent parent/child collection
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    public interface TimeDependentParentChildCollection<TParent, TChild> where TParent:class where TChild:class
    {
        /// <summary>
        /// <inheritdoc cref="TimeDependentCollectionType"/>
        /// </summary>
        public TimeDependentCollectionType CollectionType { get; set; }

        /// <summary>
        /// the single time slices
        /// </summary>
        public IList<TimeDependentParentChildRelationship<TimeDependentParentChildRelationship<TParent, TChild> TimeSlices { get; set; }
    }
}