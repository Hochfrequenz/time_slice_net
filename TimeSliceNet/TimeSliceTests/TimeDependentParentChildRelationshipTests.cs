using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    /// Tests <see cref="TimeDependentParentChildRelationship{TParent,TChild}"/>
    /// </summary>
    public class TimeDependentParentChildRelationshipTests
    {
        private class Foo
        {
        }

        private class Bar
        {
        }

        /// <summary>
        /// Tests that if no <see cref="IParentChildRelationship{TParent,TChild}.Discriminator"/> is given, the type name is used as discriminator
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
        /// Tests that if <see cref="IParentChildRelationship{TParent,TChild}.Discriminator"/> is given, it is used as discrimintor
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
    }
}