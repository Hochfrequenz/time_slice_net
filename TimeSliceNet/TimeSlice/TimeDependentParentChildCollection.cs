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
        [JsonPropertyName("allowOverlaps")] AllowOverlaps,

        /// <summary>
        ///     prevent overlapping time slices (there might be &gt;1 child assigned to a parent at a time
        /// </summary>
        [JsonPropertyName("preventOverlaps")] PreventOverlaps
    }

    /// <summary>
    ///     The simplest way to describe a time dependent parent/child collection
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    /// <typeparam name="TRelationship"></typeparam>
    public abstract class TimeDependentParentChildCollection<TRelationship, TParent, TChild> : IValidatableObject, IList<TRelationship>
        where TRelationship : IParentChildRelationship<TParent, TChild>, ITimeSlice, IValidatableObject
        where TParent : class
        where TChild : class
    {
        /// <summary>
        ///     the collection may be initialized by providing the parent that is shared among all children.
        /// </summary>
        /// <param name="commonParent"></param>
        /// <exception cref="ArgumentNullException">iff <paramref name="commonParent" /> is null</exception>
        protected TimeDependentParentChildCollection(TParent commonParent)
        {
            CommonParent = commonParent ?? throw new ArgumentNullException(nameof(commonParent));
            TimeSlices = new List<TRelationship>();
        }

        /// <summary>
        ///     <inheritdoc cref="TimeDependentCollectionType" />
        ///     This has to be set by the inheriting class
        /// </summary>
        /// <remarks>This is also the reason why this class is abstract. It should prevent the user of the library from suddenly changing the Collection Type which is not intended.</remarks>
        public abstract TimeDependentCollectionType CollectionType { get; }

        /// <summary>
        ///     The common parent for this collection that is the same for all slices in <see cref="TimeSlices" />
        /// </summary>
        public TParent CommonParent { get; set; }

        /// <summary>
        ///     the single time slices.
        /// </summary>
        public IList<TRelationship> TimeSlices { get; protected set; }

        /// <inheritdoc />
        public IEnumerator<TRelationship> GetEnumerator()
        {
            return TimeSlices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TimeSlices.GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(TRelationship item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (item.Parent != CommonParent)
                throw new ArgumentException($"The {nameof(item.Parent)} must be the same as this collections {nameof(CommonParent)} but {item.Parent}!={CommonParent}",
                    nameof(item));
            TimeSlices.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            TimeSlices.Clear();
        }

        /// <inheritdoc />
        public bool Contains(TRelationship item)
        {
            return TimeSlices.Contains(item);
        }


        /// <inheritdoc />
        public void CopyTo(TRelationship[] array, int arrayIndex)
        {
            TimeSlices.CopyTo(array, arrayIndex);
        }


        /// <inheritdoc />
        public bool Remove(TRelationship item)
        {
            return TimeSlices.Remove(item);
        }


        /// <inheritdoc />
        public int Count => TimeSlices.Count;

        /// <inheritdoc />
        public bool IsReadOnly => TimeSlices.IsReadOnly;

        /// <inheritdoc />
        public int IndexOf(TRelationship item)
        {
            return TimeSlices.IndexOf(item);
        }


        /// <inheritdoc />
        public void Insert(int index, TRelationship item)
        {
            TimeSlices.Insert(index, item);
        }


        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            TimeSlices.RemoveAt(index);
        }


        /// <inheritdoc />
        public TRelationship this[int index]
        {
            get => TimeSlices[index];
            set => TimeSlices[index] = value;
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
    }
}