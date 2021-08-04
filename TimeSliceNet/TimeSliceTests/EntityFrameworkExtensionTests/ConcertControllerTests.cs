using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleClasses.Music;
using ExampleWebApplication;
using ExampleWebApplication.Controllers;
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
                Assert.IsInstanceOf<OkObjectResult>(response);
                Assert.AreEqual(2, ((response as OkObjectResult).Value as List<Concert>).Count);
                var museAtRockInRio = ((response as OkObjectResult).Value as List<Concert>).Single(c => c.CommonParent.Name == "Muse");
                Assert.AreEqual(3, museAtRockInRio.TimeSlices.Count);
                Assert.IsTrue(museAtRockInRio.TimeSlices.Any(ts => ts.Child.Name == "Joao"));
                Assert.IsTrue(museAtRockInRio.TimeSlices.Any(ts => ts.Child.Name == "Carlos"));
                Assert.IsTrue(museAtRockInRio.TimeSlices.Any(ts => ts.Child.Name == "Patricia"));
                var ironMaidenAtRockInRio = ((response as OkObjectResult).Value as List<Concert>).Single(c => c.CommonParent.Name == "Iron Maiden");
                Assert.AreEqual(1, ironMaidenAtRockInRio.TimeSlices.Count);
                Assert.IsTrue(museAtRockInRio.TimeSlices.Any(ts => ts.Child.Name == "Joao"));
                Assert.AreEqual(museAtRockInRio.TimeSlices.Single(ts => ts.Child.Name == "Joao").Child,
                    ironMaidenAtRockInRio.TimeSlices.Single(ts => ts.Child.Name == "Joao").Child);
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
            Assert.IsFalse(invalidCollection.IsValid());
            using (ContextIsInUseSemaphore)
            {
                context.BackstageMeetings.Add(invalidCollection);
                Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            }
        }
    }
}