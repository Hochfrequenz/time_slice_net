using System;
using System.Text.Json;
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
            Assert.AreEqual(tdpcr, deserializedTdpcr);
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
            Assert.AreNotEqual(tdpcr, null);
            Assert.IsFalse(tdpcr.Equals(null));
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
            Assert.AreNotEqual(tdpcr, "asdasd");
            Assert.IsFalse(tdpcr.Equals("asdasd"));
        }
    }
}