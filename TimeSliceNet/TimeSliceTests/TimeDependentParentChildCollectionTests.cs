using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Tests <see cref="TimeDependentCollection{TRelation,TParent,TChild}" />
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
            var tsA = new FooBarRelation
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            var tsB = new FooBarRelation
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero)
                // open time slice
            };
            // they overlap in 2020
            var collection = new RelationsWithOverlaps(sharedParent);
            collection.Add(tsA);
            collection.Add(tsB);
            collection.TimeSlices.Count.ShouldBeEquivalentTo(collection.Count);
            collection.Contains(tsA).ShouldBeEquivalentTo(collection.TimeSliceList.Contains(tsA));
            collection.IndexOf(tsB).Should().Be(1);
            collection.Remove(tsB);
            collection.Count.Should().Be(1);
            collection.Clear();
            collection.Contains(tsA).Should().BeFalse();
            collection.Count.Should().Be(0);
        }

        /// <summary>
        ///     Test that the overlap check works
        /// </summary>
        [Test]
        public void TimeDependentRelationOverlapValidation()
        {
            var sharedParent = new Foo();
            var tsA = new FooBarRelation
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
                // open time slice
            };
            var tsB = new FooBarRelation
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            // they overlap in 2020
            var relationThatAllowsOverlaps = new RelationsWithOverlaps(sharedParent, new[] { tsA });
            var relationThatForbidsOverlaps = new RelationsWithoutOverlap(sharedParent, new[] { tsA });
            // with only one slice, both kinds are valid
            Assert.IsTrue(relationThatForbidsOverlaps.IsValid());
            Assert.IsTrue(relationThatAllowsOverlaps.IsValid());

            relationThatForbidsOverlaps.Add(tsB);
            relationThatAllowsOverlaps.Add(tsB);

            // but as soon as there is an overlap, only the one with the correct kind is ok
            Assert.IsTrue(relationThatAllowsOverlaps.IsValid());
            Assert.IsFalse(relationThatForbidsOverlaps.IsValid());
        }

        /// <summary>
        ///     Test that a collection is invalid as soon as at least one element in <see cref="TimeDependentCollection{TRelation,TParent,TChild}.TimeSlices" /> is invalid
        /// </summary>
        [Test]
        public void TestValidationErrorsAreForwarded()
        {
            var sharedParent = new Foo();
            var validTimeSlice = new FooBarRelation
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
                // open time slice
            };
            validTimeSlice.IsValid().Should().BeTrue();
            var invalidTimeSlice = new FooBarRelation
            {
                Parent = sharedParent,
                Child = new Bar(),
                Start = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            invalidTimeSlice.IsValid().Should().BeFalse();
            var relationThatAllowsOverlaps = new RelationsWithOverlaps(sharedParent, new[] { validTimeSlice });

            relationThatAllowsOverlaps.IsValid().Should().BeTrue();
            relationThatAllowsOverlaps.Add(invalidTimeSlice);
            relationThatAllowsOverlaps.IsValid().Should().BeFalse();

            var initiallyInvalidCollection = new RelationsWithOverlaps(sharedParent, new[] { invalidTimeSlice });
            initiallyInvalidCollection.IsValid().Should().BeFalse();
        }


        /// <summary>
        ///     Test that you cannot add null value as parent
        /// </summary>
        [Test]
        public void TestParentMustNotBeNull()
        {
            Action addingNullAsParent = () => _ = new RelationsWithOverlaps(null);
            addingNullAsParent.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        ///     Test that you cannot add null values to the collection time slices.
        /// </summary>
        [Test]
        public void TestChildMustNotBeNull()
        {
            var rs = new RelationsWithOverlaps(new Foo());
            Action nullAdd = () => rs.Add(null);
            nullAdd.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        ///     Test that all the slices must have the same parent (and everything else is rejected)
        /// </summary>
        [Test]
        public void TestAllSlicesMustHaveSameParent()
        {
            var theRightParent = new Foo();
            var theWrongParent = new Foo();
            var collection = new RelationsWithOverlaps(theRightParent);
            var sliceWithWrongParent = new FooBarRelation
            {
                Parent = theWrongParent,
                Child = new Bar()
            };
            Action invalidAdd = () => collection.Add(sliceWithWrongParent);
            invalidAdd.ShouldThrow<ArgumentException>();
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
            var collection = new RelationsWithOverlaps(foo);
            collection.Add(new FooBarRelation
            {
                Parent = foo,
                Child = new Bar
                {
                    BarName = "qwe"
                },
                Start = DateTimeOffset.UtcNow,
                End = DateTimeOffset.UtcNow.AddHours(2)
            });
            collection.Add(new FooBarRelation
            {
                Parent = foo,
                Child = new Bar
                {
                    BarName = "rtz"
                },
                Start = DateTimeOffset.UtcNow.AddHours(1),
                End = DateTimeOffset.UtcNow.AddHours(3)
            });
            collection.Equals(collection).Should().BeTrue();
            collection.Equals((object)collection).Should().BeTrue();
            collection.Equals((RelationsWithOverlaps)null).Should().BeFalse();
            collection.Equals((object)null).Should().BeFalse();
            var json = JsonSerializer.Serialize(collection);
            var deserializedCollection = JsonSerializer.Deserialize<RelationsWithOverlaps>(json);
            deserializedCollection.Should().NotBeNull();
            // ReSharper disable once PossibleNullReferenceException
            deserializedCollection.CollectionType.Should().Be(collection.CollectionType);
            deserializedCollection.Count.ShouldBeEquivalentTo(collection.Count);
            deserializedCollection.CommonParent.ShouldBeEquivalentTo(collection.CommonParent);
            deserializedCollection.TimeSlices[0].ShouldBeEquivalentTo(collection.TimeSlices[0]);
            deserializedCollection.TimeSlices[1].ShouldBeEquivalentTo(collection.TimeSlices[1]);
            deserializedCollection.ShouldBeEquivalentTo(collection);
            deserializedCollection.TimeSlices.GetHashCode().Should().NotBe(collection.TimeSlices.GetHashCode());
            deserializedCollection.GetHashCode().Should().NotBe(collection.GetHashCode());
        }

        private class FooBarRelation : TimeDependentRelation<Foo, Bar>, IEquatable<FooBarRelation>
        {
            public bool Equals(FooBarRelation other)
            {
                return base.Equals(other);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class RelationsWithOverlaps : TimeDependentCollection<FooBarRelation, Foo, Bar>, IEquatable<RelationsWithOverlaps>
        {
            public RelationsWithOverlaps(Foo commonParent, IEnumerable<FooBarRelation> relations = null) : base(commonParent, relations)
            {
            }

            public RelationsWithOverlaps()
            {
            }

            public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.AllowOverlaps;

            public bool Equals(RelationsWithOverlaps other)
            {
                return base.Equals(other);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class RelationsWithoutOverlap : TimeDependentCollection<FooBarRelation, Foo, Bar>
        {
            public RelationsWithoutOverlap(Foo commonParent, IEnumerable<FooBarRelation> relations = null) : base(commonParent, relations)
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
            return obj.GetType() == GetType() && Equals((Foo)obj);
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
            return obj.GetType() == GetType() && Equals((Bar)obj);
        }

        public override int GetHashCode()
        {
            return BarName != null ? BarName.GetHashCode() : 0;
        }
    }
}