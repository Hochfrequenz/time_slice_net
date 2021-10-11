using System;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Tests <see cref="TimeDependentRelation{TParent,TChild}" />
    /// </summary>
    public class TimeDependentRelationTests
    {
        /// <summary>
        ///     Tests that if no <see cref="IRelation{TParent,TChild}.Discriminator" /> is given, the type name is used as discriminator
        /// </summary>
        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void TimeDependentRelationDefaultDiscriminator(string discriminator)
        {
            var tdpcr = new TimeDependentRelation<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = discriminator
            };
            Assert.AreEqual(tdpcr.GetType().FullName, tdpcr.Discriminator);
        }

        /// <summary>
        ///     Tests that if <see cref="IRelation{TParent,TChild}.Discriminator" /> is given, it is used as discriminator
        /// </summary>
        [Test]
        public void TimeDependentRelationCustomDiscriminator()
        {
            var tdpcr = new TimeDependentRelation<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = "foo_parent_bar_child"
            };
            Assert.AreEqual("foo_parent_bar_child", tdpcr.Discriminator);
        }

        /// <summary>
        ///     Test that (de)serialization works
        /// </summary>
        [Test]
        public void TimeDependentRelationDeSerialization()
        {
            var tdpcr = new TimeDependentRelation<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = "foo_parent_bar_child",
                Start = new DateTimeOffset(2021, 12, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            var json = JsonSerializer.Serialize(tdpcr);
            var deserializedTdpcr = JsonSerializer.Deserialize<TimeDependentRelation<Foo, Bar>>(json);
            deserializedTdpcr.Should().NotBeNull();
            deserializedTdpcr.ShouldBeEquivalentTo(tdpcr);
        }

        [Test]
        public void TestNullIsNotEqualToRelation()
        {
            var tdpcr = new TimeDependentRelation<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = "foo_parent_bar_child",
                Start = new DateTimeOffset(2021, 12, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            tdpcr.Should().NotBeNull();
            tdpcr.Equals(null).Should().BeFalse();
        }

        [Test]
        public void TestSomethingElseIsNotEqualToRelation()
        {
            var tdpcr = new TimeDependentRelation<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = "foo_parent_bar_child",
                Start = new DateTimeOffset(2021, 12, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            tdpcr.Should().NotBe("asdasd");
            // ReSharper disable once SuspiciousTypeConversion.Global
            tdpcr.Equals("asdasd").Should().BeFalse();
        }

        [Test]
        public void TestRelationShouldBeEqualToItself()
        {
            var tdpcr = new TimeDependentRelation<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = "foo_parent_bar_child",
                Start = new DateTimeOffset(2021, 12, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            tdpcr.Equals(tdpcr).Should().BeTrue();
            tdpcr.Equals((object)tdpcr).Should().BeTrue();
            tdpcr.Equals((object)null).Should().BeFalse();
            tdpcr.Equals((TimeDependentRelation<Foo, Bar>)null).Should().BeFalse();
        }

        [Test]
        public void TestRelationShouldHaveHashCodeThatDependsOnProperties()
        {
            var tdpcr = new TimeDependentRelation<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = "foo_parent_bar_child",
                Start = new DateTimeOffset(2021, 12, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            tdpcr.GetHashCode().Should().NotBe(0);

            var anotherTdpcr = new TimeDependentRelation<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = "asd",
                Start = new DateTimeOffset(2021, 12, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            tdpcr.GetHashCode().Should().NotBe(anotherTdpcr.GetHashCode());
        }
    }
}