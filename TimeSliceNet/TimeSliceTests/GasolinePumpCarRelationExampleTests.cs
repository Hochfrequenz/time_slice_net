﻿using System;
using ExampleClasses.GasStation;
using FluentAssertions;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Test cases that are not thought to cover every edge case but rather demonstrate the main ideas of this library.
    ///     In this test class we're dealing with <see cref="TimeDependentRelation{TParent,TChild}" />s.
    /// </summary>
    public class GasolinePumpCarRelationExampleTests
    {
        /// <summary>
        ///     demonstrates the use of the <see cref="TimeDependentRelation{TParent,TChild}" />
        /// </summary>
        [Test]
        [TestCase("2021-08-01T11:00:00Z", false)] // before arriving at the pump
        [TestCase("2021-08-01T12:00:00Z", true)] // the car just arrived next to the pump
        [TestCase("2021-08-01T12:02:30Z", true)] // while the fuel is flowing
        [TestCase("2021-08-01T12:05:00Z", false)] // the moment the car leaves
        [TestCase("2021-08-01T12:45:00Z", false)] // after
        public void TestGasolinePumpCarRelation(string dateTimeString, bool expectedAtGasolinePump)
        {
            var myCar = new Car();
            var gasolinePump = new GasolinePump();
            // if I stayed at the gasoline pump for five minutes, the allocation looks like this:
            var gasolinePumpAllocation = new GasolinePumpAllocation
            {
                Start = new DateTimeOffset(2021, 8, 1, 12, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 12, 5, 0, TimeSpan.Zero),
                Parent = gasolinePump,
                Child = myCar
            };
            gasolinePumpAllocation.Duration.Should().Be(TimeSpan.FromMinutes(5));
            var myCarOccupiesTheGasolinePump = gasolinePumpAllocation.Overlaps(DateTimeOffset.Parse(dateTimeString));
            myCarOccupiesTheGasolinePump.Should().Be(expectedAtGasolinePump);
        }
    }
}