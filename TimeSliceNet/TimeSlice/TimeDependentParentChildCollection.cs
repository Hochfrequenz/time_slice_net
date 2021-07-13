using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        [JsonPropertyName("allowOverlaps")] AllowOverlaps,

        /// <summary>
        /// prevent overlapping time slices (there might be &gt;1 child assigned to a parent at a time
        /// </summary>
        [JsonPropertyName("preventOverlaps")] PreventOverlaps
    }

    /// <summary>
    /// The simplest way to describe a time dependent parent/child collection
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    public abstract class TimeDependentParentChildCollection<TParent, TChild> : IValidatableObject where TParent : class where TChild : class
    {
        /// <summary>
        /// <inheritdoc cref="TimeDependentCollectionType"/>
        /// This has to be set by the inheriting class
        /// </summary>
        public abstract TimeDependentCollectionType CollectionType { get; }

        /// <summary>
        /// the single time slices.
        /// </summary>
        public IList<TimeDependentParentChildRelationship<TParent, TChild>> TimeSlices { get; set; }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            foreach (var ts in TimeSlices)
            {
                results.AddRange(ts.Validate(validationContext));
            }
            if (TimeSlices is not { Count: > 1 })
            {
                // no further checks required
                return results;
            }
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