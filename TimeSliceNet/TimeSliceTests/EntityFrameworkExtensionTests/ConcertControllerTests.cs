using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleClasses.Festival;
using ExampleWebApplication;
using ExampleWebApplication.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests.EntityFrameworkExtensionTests
{
    public class ConcertControllerTests : ControllerTest
    {
        public ConcertControllerTests()
            : base(
                new DbContextOptionsBuilder<TimeSliceContext>()
                    .UseSqlite(CreateInMemoryDatabase())
                    .Options)
        {
        }

        [Test]
        [NonParallelizable]
        public async Task TestGetAll()
        {
            using (ContextIsInUseSemaphore)
            {
                var controller = new ConcertController(context);
                // test the concerts
                var response = await controller.GetAllConcerts("rio");
                response.Should().BeOfType<OkObjectResult>().Subject.Value.Should().BeOfType<List<Concert>>().Subject.Should().HaveCount(2);
                var museAtRockInRio = ((response as OkObjectResult).Value as List<Concert>).Single(c => c.CommonParent.Name == "Muse");
                museAtRockInRio.IsValid.Should().BeTrue(); // <-- property, not method
                museAtRockInRio.IsValid = false; // <-- has no effect
                museAtRockInRio.IsValid.Should().BeTrue(); // <-- property, not method
                museAtRockInRio.Equals(null).Should().BeFalse();
                // ReSharper disable once SuspiciousTypeConversion.Global
                museAtRockInRio.Equals("null").Should().BeFalse();
                var concertDirectlyFromContext = await context.Concerts.SingleAsync(c => c.CommonParentId == "Muse");
                museAtRockInRio.ShouldBeEquivalentTo(concertDirectlyFromContext);
                concertDirectlyFromContext.Equals(museAtRockInRio).Should().BeTrue();
                concertDirectlyFromContext.GetHashCode().ShouldBeEquivalentTo(museAtRockInRio.GetHashCode());
                museAtRockInRio.TimeSlices.Should().HaveCount(3);
                museAtRockInRio.TimeSlices.Should().Contain(cv => cv.Child.Name == "Joao");
                museAtRockInRio.TimeSlices.Should().Contain(cv => cv.Child.Name == "Carlos");
                museAtRockInRio.TimeSlices.Should().Contain(cv => cv.Child.Name == "Patricia");
                var ironMaidenAtRockInRio = ((response as OkObjectResult).Value as List<Concert>).Single(c => c.CommonParent.Name == "Iron Maiden");
                Assert.AreEqual(1, ironMaidenAtRockInRio.TimeSlices.Count);
                ironMaidenAtRockInRio.TimeSlices.Should().HaveCount(1);
                museAtRockInRio.TimeSlices.Should().Contain(cv => cv.Child.Name == "Joao");
                ironMaidenAtRockInRio.TimeSlices.Single(ts => ts.Child.Name == "Joao").Child.Should().Be(museAtRockInRio.TimeSlices.Single(ts => ts.Child.Name == "Joao").Child);
            }
        }

        [Test]
        [NonParallelizable]
        public async Task InvalidEntriesMustNotBeAdded()
        {
            var star = new Musician
            {
                Name = "This musician only has time for 1 fan at a time"
            };
            var fanA = new Listener
            {
                Name = "Fan A"
            };
            var fanB = new Listener
            {
                Name = "Fan B"
            };
            var invalidCollection = new BackstageMeetings
            {
                CommonParent = star,
                TimeSlices =
                {
                    new OneOnOneWithAStar
                    {
                        Start = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2021, 1, 1, 1, 0, 0, TimeSpan.Zero),
                        Parent = star,
                        Child = fanA
                    },
                    new OneOnOneWithAStar
                    {
                        // this time slices conflicts with the one on one with a star of fan A
                        Start = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2021, 1, 1, 1, 0, 0, TimeSpan.Zero),
                        Parent = star,
                        Child = fanB
                    }
                }
            };
            invalidCollection.IsValid().Should().BeFalse();
            using (ContextIsInUseSemaphore)
            {
                await context.BackstageMeetings.AddAsync(invalidCollection);
                Action invalidSave = () => context.SaveChanges();
                invalidSave.ShouldThrow<DbUpdateException>();
            }
        }
    }
}