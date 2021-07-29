using System;
using System.Text.Json;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Tests <see cref="TimeDependentParentChildRelationship{TParent,TChild}" />
    /// </summary>
    public class TimeDependentParentChildRelationshipTests
    {
        /// <summary>
        ///     Tests that if no <see cref="IParentChildRelationship{TParent,TChild}.Discriminator" /> is given, the type name is used as discriminator
        /// </summary>
        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void TimeDependentParentChildRelationshipDefaultDiscriminator(string discriminator)
        {
            var tdpcr = new TimeDependentParentChildRelationship<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = discriminator
            };
            Assert.AreEqual(tdpcr.GetType().FullName, tdpcr.Discriminator);
        }

        /// <summary>
        ///     Tests that if <see cref="IParentChildRelationship{TParent,TChild}.Discriminator" /> is given, it is used as discriminator
        /// </summary>
        [Test]
        public void TimeDependentParentChildRelationshipCustomDiscriminator()
        {
            var tdpcr = new TimeDependentParentChildRelationship<Foo, Bar>
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
        public void TimeDependentParentChildRelationshipDeSerialization()
        {
            var tdpcr = new TimeDependentParentChildRelationship<Foo, Bar>
            {
                Child = new Bar(),
                Parent = new Foo(),
                Discriminator = "foo_parent_bar_child",
                Start = new DateTimeOffset(2021, 12, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };
            var json = JsonSerializer.Serialize(tdpcr);
            var deserializedTdpcr = JsonSerializer.Deserialize<TimeDependentParentChildRelationship<Foo, Bar>>(json);
            Assert.AreEqual(tdpcr, deserializedTdpcr);
        }
    }
}