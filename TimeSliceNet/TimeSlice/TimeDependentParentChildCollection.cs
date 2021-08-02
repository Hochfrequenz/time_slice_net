using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace TimeSlice
{
    /// <summary>
    ///     Describes the possible kinds of time dependent relationships between parents and children
    /// </summary>
    public enum TimeDependentCollectionType
    {
        /// <summary>
        ///     &lt;= 1 child assigned to a parent at a time
        /// </summary>
        AllowOverlaps,

        /// <summary>
        ///     prevent overlapping time slices (there might be &gt;1 child assigned to a parent at a time)
        /// </summary>
        PreventOverlaps
    }

    /// <summary>
    ///     The simplest way to describe a time dependent parent/child collection
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    /// <typeparam name="TRelationship"></typeparam>
    public abstract class TimeDependentParentChildCollection<TRelationship, TParent, TChild> : IEquatable<TimeDependentParentChildCollection<TRelationship, TParent, TChild>>,
        IValidatableObject
        where TRelationship : IParentChildRelationship<TParent, TChild>, ITimeSlice, IValidatableObject // IEquatable<TRelationship>
        where TParent : class
        where TChild : class
    {
        /// <summary>
        ///     the collection may be initialized by providing the parent that is shared among all children.
        /// </summary>
        /// <param name="commonParent"></param>
        /// <param name="relationships">optional relationships to be added on construction</param>
        /// <exception cref="ArgumentNullException">iff <paramref name="commonParent" /> is null</exception>
        protected TimeDependentParentChildCollection(TParent commonParent, IEnumerable<TRelationship> relationships = null) : this()
        {
            CommonParent = commonParent ?? throw new ArgumentNullException(nameof(commonParent));
            if (relationships == null) return;
            foreach (var r in relationships)
                TimeSlices.Add(r);
        }

        /// <summary>
        ///     A parameterless constructor is required for deserializing
        /// </summary>
        protected TimeDependentParentChildCollection()
        {
            TimeSlices = new List<TRelationship>();
        }

        /// <summary>
        ///     <inheritdoc cref="TimeDependentCollectionType" />
        ///     This has to be set by the inheriting class
        /// </summary>
        /// <remarks>This is also the reason why this class is abstract. It should prevent the user of the library from suddenly changing the Collection Type which is not intended.</remarks>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public abstract TimeDependentCollectionType CollectionType { get; }

        /// <summary>
        ///     The common parent for this collection that is the same for all slices in <see cref="TimeSlices" />
        /// </summary>
        public TParent CommonParent { get; init; }

        /// <summary>
        ///     the single time slices.
        /// </summary>
        /// <remarks>
        ///     We model a list of <typeparamref name="TRelationship" /> instead of <typeparamref name="TChild" /> because each of the items inside this list may still be persisted or
        ///     serialized on its own without "knowing" about the other items.
        /// </remarks>
        [JsonInclude]
        public IList<TRelationship> TimeSlices { get; protected set; }

        /// <summary>
        ///     returns a list of all children time slices as list that is sorted by start and end
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public List<TRelationship> TimeSliceList => TimeSlices.OrderBy(ts => (ts.Start, ts.End ?? DateTimeOffset.MaxValue)).ToList();


        /// <inheritdoc cref="ICollection.Count" />
        public int Count => TimeSlices.Count;

        /// <inheritdoc />
        public bool Equals(TimeDependentParentChildCollection<TRelationship, TParent, TChild> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CollectionType == other.CollectionType && EqualityComparer<TParent>.Default.Equals(CommonParent, other.CommonParent) &&
                   TimeSlices.SequenceEqual(other.TimeSlices);
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            foreach (var ts in TimeSlices) results.AddRange(ts.Validate(validationContext));
            if (TimeSlices is not { Count: > 1 })
                // no further checks required
                return results;
            switch (CollectionType)
            {
                case TimeDependentCollectionType.AllowOverlaps:
                    // no checks so far
                    break;
                case TimeDependentCollectionType.PreventOverlaps:
                    // check that the time slices do not overlap
                    var overlappingSlices = from tsA in TimeSlices
                                            from tsB in TimeSlices
                                            where !ReferenceEquals(tsA, tsB) && tsA.Overlaps(tsB)
                                            select (tsA, tsB);
                    if (overlappingSlices.Any())
                    {
                        var errorMessage =
                            $"The following time slices overlap: {string.Join(", ", overlappingSlices.Select(tuple => $"({tuple.tsA}, {tuple.tsB})"))}";
                        results.Add(new ValidationResult(errorMessage, new List<string> { nameof(TimeSlices) }));
                    }

                    // check is ok => no overlaps
                    break;
                default:
                    throw new NotImplementedException($"The handling of {CollectionType} is not implemented.");
            }

            return results;
        }


        /// <summary>
        ///     Add an item to the children
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException">iff <paramref name="item" /> is null</exception>
        /// <exception cref="ArgumentException">if the parent of <paramref name="item" /> is not the same as <see cref="CommonParent" /></exception>
        public void Add(TRelationship item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (CommonParent != null && item.Parent != CommonParent)
                throw new ArgumentException($"The {nameof(item.Parent)} must be the same as this collections {nameof(CommonParent)} but {item.Parent}!={CommonParent}",
                    nameof(item));
            TimeSlices.Add(item);
        }

        /// <inheritdoc cref="IList.Clear" />
        public void Clear()
        {
            TimeSlices.Clear();
        }

        /// <inheritdoc cref="IList.Contains" />
        public bool Contains(TRelationship item)
        {
            return TimeSlices.Contains(item);
        }


        /// <inheritdoc cref="IList.Remove" />
        public bool Remove(TRelationship item)
        {
            return TimeSlices.Remove(item);
        }


        /// <inheritdoc cref="IList.IndexOf" />
        public int IndexOf(TRelationship item)
        {
            return TimeSlices.IndexOf(item);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TimeDependentParentChildCollection<TRelationship, TParent, TChild>)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine((int)CollectionType, CommonParent, TimeSlices);
        }
    }
}