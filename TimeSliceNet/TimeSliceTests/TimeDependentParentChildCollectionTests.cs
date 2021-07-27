using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    /// Tests <see cref="TimeDependentParentChildCollection{TParent,TChild}"/>
    /// </summary>
    public class TimeDependentParentChildCollectionTests
    {
        private class Foo
        {
        }

        private class Bar
        {
        }

        private class RelationshipsWithOverlaps : TimeDependentParentChildCollection<Foo, Bar>
        {
            public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.AllowOverlaps;
        }

        private class RelationshipsWithoutOverlaps : TimeDependentParentChildCollection<Foo, Bar>
        {
            public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.PreventOverlaps;
        }


        /// <summary>
        /// Test that the overlap check works
        /// </summary>
        [Test]
        public void TimeDependentParentChildRelationshipOverlapValidation()
        {
            var tsA = new TimeDependentParentChildRelationship<Foo, Bar>()
            {
                Parent = new Foo(),
                Child = new Bar(),
                Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                // open time slice
            };
            var tsB = new TimeDependentParentChildRelationship<Foo, Bar>()
            {
                Parent = new Foo(),
                Child = new Bar(),
                Start = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
            };
            // they overlap in 2020
            var relationshipThatAllowsOverlaps = new RelationshipsWithOverlaps()
            {
                TimeSlices = new List<TimeDependentParentChildRelationship<Foo, Bar>>()
                {
                    tsA
                }
            };

            var relationshipThatForbisOverlaps = new RelationshipsWithoutOverlaps()
            {
                TimeSlices = new List<TimeDependentParentChildRelationship<Foo, Bar>>()
                {
                    tsA
                }
            };
            // with only one slice, both kinds are valid
            Assert.IsFalse(relationshipThatForbisOverlaps.Validate(null).Any());
            Assert.IsFalse(relationshipThatAllowsOverlaps.Validate(null).Any());

            relationshipThatForbisOverlaps.TimeSlices.Add(tsB);
            relationshipThatAllowsOverlaps.TimeSlices.Add(tsB);

            // but as soon as there is an overlap, only the one with the correct kind is ok
            Assert.IsFalse(relationshipThatAllowsOverlaps.Validate(null).Any());
            Assert.IsTrue(relationshipThatForbisOverlaps.Validate(null).Any());
        }

        /// <summary>
        /// Test that a collection is invalid as soon as at least one element in <see cref="TimeDependentParentChildCollection{TParent,TChild}.TimeSlices"/> is invalid
        /// </summary>
        [Test]
        public void TestValidationErrorsAreForwarded()
        {
            var validTimeSlice = new TimeDependentParentChildRelationship<Foo, Bar>()
            {
                Parent = new Foo(),
                Child = new Bar(),
                Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                // open time slice
            };
            Assert.IsFalse(validTimeSlice.Validate(null).Any());
            var invalidTimeSlice = new TimeDependentParentChildRelationship<Foo, Bar>()
            {
                Parent = new Foo(),
                Child = new Bar(),
                Start = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero),
            };
            Assert.IsTrue(invalidTimeSlice.Validate(null).Any());
            var relationshipThatAllowsOverlaps = new RelationshipsWithOverlaps()
            {
                TimeSlices = new List<TimeDependentParentChildRelationship<Foo, Bar>>()
                {
                    validTimeSlice
                }
            };
            Assert.IsFalse(relationshipThatAllowsOverlaps.Validate(null).Any());
            relationshipThatAllowsOverlaps.TimeSlices.Add(invalidTimeSlice);
            Assert.IsTrue(relationshipThatAllowsOverlaps.Validate(null).Any());

            var initiallyInvalidCollection = new RelationshipsWithOverlaps()
            {
                TimeSlices = new List<TimeDependentParentChildRelationship<Foo, Bar>>()
                {
                    invalidTimeSlice
                }
            };
            Assert.IsTrue(initiallyInvalidCollection.Validate(null).Any());
        }
    }
}