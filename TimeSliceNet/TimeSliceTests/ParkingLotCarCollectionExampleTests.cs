using System;
using NUnit.Framework;
using TimeSlice;
using static TimeSliceTests.ParkingLotCarRelationshipExampleTests;

namespace TimeSliceTests
{
    /// <summary>
    ///     Test cases that are not thought to cover every edge case but rather demonstrate the main ideas of this library.
    ///     In this test class we cover <see cref="TimeDependentParentChildCollection{TRelationship, TParent,TChild}" />s.
    /// </summary>
    /// <remarks>To understand these tests, first have a look at <seealso cref="ParkingLotCarRelationshipExampleTests" /></remarks>
    public class ParkingLotCarCollectionExampleTests
    {
        /// <summary>
        ///     demonstrates the use of the <see cref="TimeDependentParentChildCollection{TRelationship,TParent,TChild}" />
        /// </summary>
        [Test]
        public void TestParkingLotAllocationCollection()
        {
            var myCar = new Car();
            var otherCar = new Car();
            var superMarketParkingLot = new ParkingLot();
            // if I parked my car at the super market for half an hour, the allocation looks like this:
            var myParkingLotAllocation = new ParkingLotAllocation
            {
                Start = new DateTimeOffset(2021, 8, 1, 12, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 12, 30, 0, TimeSpan.Zero),
                Parent = superMarketParkingLot,
                Child = myCar
            };
            var anotherParkingLotAllocation = new ParkingLotAllocation
            {
                Start = new DateTimeOffset(2021, 8, 1, 12, 45, 0, TimeSpan.Zero), // a bit later
                End = new DateTimeOffset(2021, 8, 1, 13, 00, 0, TimeSpan.Zero),
                Parent = superMarketParkingLot, // same parking lot
                Child = otherCar // but different car
            };
            // collections are initialized by providing the common parent (the "1" in the 1:n cardinality)
            var collection = new ParkingLotAllocationCollection(superMarketParkingLot)
            {
                myParkingLotAllocation, // this describes, that my car uses the super market parking lot
                anotherParkingLotAllocation // another car may use the same parking lot at a different time
            };
            // ...12:00....12:30..12:45.....13:15. ....... (time) --->
            // .....|XXXXXXXX|.....|XXXXXXXXXX|........... (parking lot is occupied)
            //      [ my car )
            //                     [other car)
            Assert.IsTrue(collection.IsValid()); // collections, that don't allow overlaps are valid if there are no overlaps

            // we can try to park a car in a time frame that overlaps with another allocation
            var conflictingParkingLotAllocation = new ParkingLotAllocation
            {
                Start = new DateTimeOffset(2021, 8, 1, 12, 15, 0, TimeSpan.Zero), // overlap => conflict
                End = new DateTimeOffset(2021, 8, 1, 12, 20, 0, TimeSpan.Zero),
                Parent = superMarketParkingLot, // same parking lot
                Child = new Car()
            };
            collection.Add(conflictingParkingLotAllocation);
            Assert.IsFalse(collection.IsValid());
        }

        /// <summary>
        ///     multiple allocations that vary over time are modeled as a "collection".
        /// </summary>
        internal class ParkingLotAllocationCollection : TimeDependentParentChildCollection<ParkingLotAllocation, ParkingLot, Car>
        {
            public ParkingLotAllocationCollection(ParkingLot commonParent) : base(commonParent)
            {
            }

            /// <summary>
            ///     one parking lot can only be used by one car at a time => temporal overlaps are not allowed.
            /// </summary>
            public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.PreventOverlaps;
        }
    }
}