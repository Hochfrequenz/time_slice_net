using System;
using System.Collections.Generic;
using System.Text.Json;
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
            var collection = new RelationshipsWithOverlaps(sharedParent);
            collection.Add(tsA);
            collection.Add(tsB);
            Assert.AreEqual(collection.Count, collection.TimeSliceList.Count);
            Assert.AreEqual(collection.Contains(tsA), collection.TimeSliceList.Contains(tsA));
            Assert.AreEqual(1, collection.IndexOf(tsB));
            collection.Remove(tsB);
            Assert.AreEqual(1, collection.Count);
            collection.Clear();
            Assert.IsFalse(collection.Contains(tsA));
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
            var relationshipThatAllowsOverlaps = new RelationshipsWithOverlaps(sharedParent, new[] {tsA});
            var relationshipThatForbidsOverlaps = new RelationshipsWithoutOverlaps(sharedParent, new[] {tsA});
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
        ///     Test that a collection is invalid as soon as at least one element in <see cref="TimeDependentParentChildCollection{TRelationship,TParent,TChild}.TimeSlices" /> is invalid
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
            var relationshipThatAllowsOverlaps = new RelationshipsWithOverlaps(sharedParent, new[] {validTimeSlice});

            Assert.IsTrue(relationshipThatAllowsOverlaps.IsValid());
            relationshipThatAllowsOverlaps.Add(invalidTimeSlice);
            Assert.IsFalse(relationshipThatAllowsOverlaps.IsValid());

            var initiallyInvalidCollection = new RelationshipsWithOverlaps(sharedParent, new[] {invalidTimeSlice});
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
            var rs = new RelationshipsWithOverlaps(new Foo());
            Assert.Throws<ArgumentNullException>(() => rs.Add(null));
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

        /// <summary>
        ///     Test, that collections serialize and deserialize easily
        /// </summary>
        [Test]
        public void TestCollectionDeserialization()
        {
            var foo = new Foo
            {
                FooName = "asd"
            };
            var collection = new RelationshipsWithOverlaps(foo);
            collection.Add(new FooBarRelationship
            {
                Parent = foo,
                Child = new Bar
                {
                    BarName = "qwe"
                },
                Start = DateTimeOffset.UtcNow,
                End = DateTimeOffset.UtcNow.AddHours(2)
            });
            collection.Add(new FooBarRelationship
            {
                Parent = foo,
                Child = new Bar
                {
                    BarName = "rtz"
                },
                Start = DateTimeOffset.UtcNow.AddHours(1),
                End = DateTimeOffset.UtcNow.AddHours(3)
            });
            var json = JsonSerializer.Serialize(collection);
            var deserializedCollection = JsonSerializer.Deserialize<RelationshipsWithOverlaps>(json);
            Assert.IsNotNull(deserializedCollection);
            Assert.AreEqual(collection.CollectionType, deserializedCollection.CollectionType);
            Assert.AreEqual(collection.Count, deserializedCollection.Count);
            Assert.AreEqual(collection.CommonParent, deserializedCollection.CommonParent);
            Assert.AreEqual(collection.TimeSlices[0], deserializedCollection.TimeSlices[0]);
            Assert.AreEqual(collection.TimeSlices[1], deserializedCollection.TimeSlices[1]);
            Assert.AreEqual(collection, deserializedCollection);

            Assert.AreEqual(collection.GetHashCode(), deserializedCollection.GetHashCode());
        }

        private class FooBarRelationship : TimeDependentParentChildRelationship<Foo, Bar>, IEquatable<FooBarRelationship>
        {
            public bool Equals(FooBarRelationship other)
            {
                return base.Equals(other);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((FooBarRelationship) obj);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }

        private class RelationshipsWithOverlaps : TimeDependentParentChildCollection<FooBarRelationship, Foo, Bar>, IEquatable<RelationshipsWithOverlaps>
        {
            public RelationshipsWithOverlaps(Foo commonParent, IEnumerable<FooBarRelationship> relationships = null) : base(commonParent, relationships)
            {
            }

            public RelationshipsWithOverlaps()
            {
            }

            public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.AllowOverlaps;

            public bool Equals(RelationshipsWithOverlaps other)
            {
                return base.Equals(other);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((RelationshipsWithOverlaps) obj);
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }

        private class RelationshipsWithoutOverlaps : TimeDependentParentChildCollection<FooBarRelationship, Foo, Bar>
        {
            public RelationshipsWithoutOverlaps(Foo commonParent, IEnumerable<FooBarRelationship> relationships = null) : base(commonParent, relationships)
            {
            }

            public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.PreventOverlaps;
        }
    }

    internal class Foo : IEquatable<Foo>
    {
        public string FooName { get; init; }

        public bool Equals(Foo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FooName == other.FooName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Foo) obj);
        }

        public override int GetHashCode()
        {
            return FooName != null ? FooName.GetHashCode() : 0;
        }
    }

    internal class Bar : IEquatable<Bar>
    {
        public string BarName { get; set; }

        public bool Equals(Bar other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BarName == other.BarName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Bar) obj);
        }

        public override int GetHashCode()
        {
            return BarName != null ? BarName.GetHashCode() : 0;
        }
    }
}