using System;
using ExampleClasses.GasStation;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Test cases that are not thought to cover every edge case but rather demonstrate the main ideas of this library.
    ///     In this test class we cover <see cref="TimeDependentParentChildCollection{TRelation, TParent,TChild}" />s.
    /// </summary>
    /// <remarks>To understand these tests, first have a look at <seealso cref="GasolinePumpCarRelationExampleTests" /></remarks>
    public class GasolinePumpCarNonOverlappingExampleTests
    {
        /// <summary>
        ///     demonstrates the use of the <see cref="TimeDependentParentChildCollection{TRelation,TParent,TChild}" /> with <see cref="TimeDependentCollectionType.PreventOverlaps" />
        /// </summary>
        [Test]
        public void TestMultipleCarsAtOneGasolinePump()
        {
            var myCar = new Car();
            var otherCar = new Car();
            var theSinglePump = new GasolinePump();
            // if my car is standing next to the only available gasoline pump for five minutes the allocation looks like this:
            var myGasolinePumpAllocation = new GasolinePumpAllocation
            {
                Start = new DateTimeOffset(2021, 8, 1, 12, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 12, 5, 0, TimeSpan.Zero),
                Parent = theSinglePump,
                Child = myCar
            };
            var anotherGasolinePumpAllocation = new GasolinePumpAllocation
            {
                Start = new DateTimeOffset(2021, 8, 1, 12, 10, 0, TimeSpan.Zero), // a bit later
                End = new DateTimeOffset(2021, 8, 1, 13, 20, 0, TimeSpan.Zero),
                Parent = theSinglePump, // same pump
                Child = otherCar // but different car
            };
            // collections are initialized by providing the common parent (the "1" in the 1:n cardinality)
            var collection = new GasolinePumpAllocationCollection(theSinglePump);
            collection.Add(myGasolinePumpAllocation); // this describes, that my car uses the gasoline pump
            collection.Add(anotherGasolinePumpAllocation); // another car may use the same gasoline pump but only at a different time
            // ...12:00....12:05..12:10.....12:20. ....... (time) --->
            // .....|XXXXXXXX|.....|XXXXXXXXXX|........... (gasoline pump is occupied)
            //      [ my car )
            //                     [other car)
            Assert.IsTrue(collection.IsValid()); // collections, that don't allow overlaps are valid if there are no overlaps

            // we can try to make another car use the same gasoline pump in a time frame that overlaps with another allocation
            var conflictingAllocation = new GasolinePumpAllocation
            {
                Start = new DateTimeOffset(2021, 8, 1, 12, 07, 0, TimeSpan.Zero), // overlap => conflict
                End = new DateTimeOffset(2021, 8, 1, 12, 12, 0, TimeSpan.Zero),
                Parent = theSinglePump, // same parking lot
                Child = new Car()
            };
            collection.Add(conflictingAllocation);
            Assert.IsFalse(collection.IsValid());
            // => if you always check for validity (e.g. before saving a collection or in some kind of middleware), you'll be fine
        }
    }
}