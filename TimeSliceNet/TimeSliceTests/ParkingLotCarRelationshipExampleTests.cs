using System;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Test cases that are not thought to cover every edge case but rather demonstrate the main ideas of this library.
    ///     In this test class we're dealing with <see cref="TimeDependentParentChildRelationship{TParent,TChild}" />s.
    /// </summary>
    public class ParkingLotCarRelationshipExampleTests
    {
        /// <summary>
        ///     demonstrates the use of the <see cref="TimeDependentParentChildRelationship{TParent,TChild}" />
        /// </summary>
        [Test]
        [TestCase("2021-08-01T11:00:00Z", false)] // before shopping
        [TestCase("2021-08-01T12:00:00Z", true)] // the moment the car arrives
        [TestCase("2021-08-01T12:15:00Z", true)] // while in the supermarket
        [TestCase("2021-08-01T12:30:00Z", false)] // the moment the car leaves
        [TestCase("2021-08-01T12:45:00Z", false)] // after shopping
        public void TestParkingLotCarRelationship(string dateTimeString, bool expectedAtSuperMarket)
        {
            var myCar = new Car();
            var superMarketParkingLot = new ParkingLot();
            // if I parked my car at the super market for half an hour, the allocation looks like this:
            var parkingLotAllocation = new ParkingLotAllocation
            {
                Start = new DateTimeOffset(2021, 8, 1, 12, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 12, 30, 0, TimeSpan.Zero),
                Parent = superMarketParkingLot,
                Child = myCar
            };
            Assert.AreEqual(TimeSpan.FromMinutes(30), parkingLotAllocation.Duration);
            var myCarIsAtTheSuperMarket = parkingLotAllocation.Overlaps(DateTimeOffset.Parse(dateTimeString));
            Assert.AreEqual(expectedAtSuperMarket, myCarIsAtTheSuperMarket);
        }

        /// <summary>
        ///     a car occupies public space (e.g. parking lots)
        /// </summary>
        internal class Car
        {
        }

        /// <summary>
        ///     a parking lot is space that can be occupied by cars, but only one car at a time.
        /// </summary>
        internal class ParkingLot
        {
        }

        /// <summary>
        ///     The allocation/usage of a parking lot by a car for a specific time frame is called "allocation".
        /// </summary>
        internal class ParkingLotAllocation : TimeDependentParentChildRelationship<ParkingLot, Car>
        {
        }
    }
}