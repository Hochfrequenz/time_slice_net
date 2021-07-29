using System;
using System.Collections.Generic;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Tests <see cref="TimeDependentParentChildCollection{TRelationship, TParent,TChild}" />
    /// </summary>
    public class TimeDependentParentChildCollectionTests
    {
        /// <summary>
        ///     Test that <see cref="IList{T}" /> is implemented correctly
        /// </summary>
        [Test]
        public void TestIListProperties()
        {
            var sharedParent = new Foo();
            var tsA = new FooBarRelationship
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            var tsB = new FooBarRelationship
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero)
                // open time slice
            };
            // they overlap in 2020
            var collection = new RelationshipsWithOverlaps(sharedParent)
            {
                tsA,
                tsB
            };
            Assert.AreEqual(collection[0], tsA);
            Assert.AreEqual(collection.Count, collection.ToTimeSliceList().Count);
            Assert.IsNotNull(collection.GetEnumerator());
            Assert.AreEqual(collection.Contains(tsA), collection.ToTimeSliceList().Contains(tsA));
            Assert.AreEqual(collection.IsReadOnly, false);
            collection.Remove(tsB);
            Assert.AreEqual(1, collection.Count);
            collection.Insert(1, tsB);
            Assert.AreEqual(1, collection.IndexOf(tsB));
            Assert.Throws<NotImplementedException>(() => collection[1] = tsB);
            collection.RemoveAt(0);
            Assert.IsFalse(collection.Contains(tsA));
            collection.Clear();
            Assert.AreEqual(0, collection.Count);
        }

        /// <summary>
        ///     Test that the overlap check works
        /// </summary>
        [Test]
        public void TimeDependentParentChildRelationshipOverlapValidation()
        {
            var sharedParent = new Foo();
            var tsA = new FooBarRelationship
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
                // open time slice
            };
            var tsB = new FooBarRelationship
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            // they overlap in 2020
            var relationshipThatAllowsOverlaps = new RelationshipsWithOverlaps(sharedParent)
            {
                tsA
            };

            var relationshipThatForbidsOverlaps = new RelationshipsWithoutOverlaps(sharedParent)
            {
                tsA
            };
            // with only one slice, both kinds are valid
            Assert.IsTrue(relationshipThatForbidsOverlaps.IsValid());
            Assert.IsTrue(relationshipThatAllowsOverlaps.IsValid());

            relationshipThatForbidsOverlaps.Add(tsB);
            relationshipThatAllowsOverlaps.Add(tsB);

            // but as soon as there is an overlap, only the one with the correct kind is ok
            Assert.IsTrue(relationshipThatAllowsOverlaps.IsValid());
            Assert.IsFalse(relationshipThatForbidsOverlaps.IsValid());
        }

        /// <summary>
        ///     Test that a collection is invalid as soon as at least one element in <see cref="TimeDependentParentChildCollection{TRelationship, TParent,TChild}.TimeSlices" /> is invalid
        /// </summary>
        [Test]
        public void TestValidationErrorsAreForwarded()
        {
            var sharedParent = new Foo();
            var validTimeSlice = new FooBarRelationship
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
                // open time slice
            };
            Assert.IsTrue(validTimeSlice.IsValid());
            var invalidTimeSlice = new FooBarRelationship
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            Assert.IsFalse(invalidTimeSlice.IsValid());
            var relationshipThatAllowsOverlaps = new RelationshipsWithOverlaps(sharedParent)
            {
                validTimeSlice
            };
            Assert.IsTrue(relationshipThatAllowsOverlaps.IsValid());
            relationshipThatAllowsOverlaps.Add(invalidTimeSlice);
            Assert.IsFalse(relationshipThatAllowsOverlaps.IsValid());

            var initiallyInvalidCollection = new RelationshipsWithOverlaps(sharedParent)
            {
                invalidTimeSlice
            };

            Assert.False(initiallyInvalidCollection.IsValid());
        }


        /// <summary>
        ///     Test that you cannot add null value as parent
        /// </summary>
        [Test]
        public void TestParentMustNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new RelationshipsWithOverlaps(null));
        }

        /// <summary>
        ///     Test that you cannot add null values to the collection time slices.
        /// </summary>
        [Test]
        public void TestChildMustNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new RelationshipsWithOverlaps(new Foo())
            {
                null
            });
        }

        /// <summary>
        ///     Test that all the slices must have the same parent (and everything else is rejected)
        /// </summary>
        [Test]
        public void TestAllSlicesMustHaveSameParent()
        {
            var theRightParent = new Foo();
            var theWrongParent = new Foo();
            var collection = new RelationshipsWithOverlaps(theRightParent);
            var sliceWithWrongParent = new FooBarRelationship
            {
                Parent = theWrongParent,
                Child = new Bar()
            };
            Assert.Throws<ArgumentException>(() => collection.Add(sliceWithWrongParent));
        }

        private class Foo
        {
        }

        private class Bar
        {
        }

        private class FooBarRelationship : TimeDependentParentChildRelationship<Foo, Bar>
        {
        }

        private class RelationshipsWithOverlaps : TimeDependentParentChildCollection<FooBarRelationship, Foo, Bar>
        {
            public RelationshipsWithOverlaps(Foo commonParent) : base(commonParent)
            {
            }

            public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.AllowOverlaps;
        }

        private class RelationshipsWithoutOverlaps : TimeDependentParentChildCollection<FooBarRelationship, Foo, Bar>
        {
            public RelationshipsWithoutOverlaps(Foo commonParent) : base(commonParent)
            {
            }

            public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.PreventOverlaps;
        }
    }
}