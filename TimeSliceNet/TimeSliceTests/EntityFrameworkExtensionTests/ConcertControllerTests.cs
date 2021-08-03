using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleClasses.Music;
using ExampleWebApplication;
using ExampleWebApplication.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

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
            await using var context = new TimeSliceContext(ContextOptions);
            await Seed(context);
            var controller = new ConcertController(context);
            // test the concerts
            var response = await controller.GetAllConcerts("rio");
            Assert.IsInstanceOf<OkObjectResult>(response);
            Assert.AreEqual(3, ((response as OkObjectResult).Value as List<Concert>).Count);
            var museAtRockInRio = ((response as OkObjectResult).Value as List<Concert>).Single(c => c.CommonParent.Name == "Muse");
            Assert.AreEqual(3, museAtRockInRio.TimeSlices.Count);
            Assert.IsTrue(museAtRockInRio.TimeSlices.Any(ts => ts.Child.Name == "Joao"));
            Assert.IsTrue(museAtRockInRio.TimeSlices.Any(ts => ts.Child.Name == "Carlos"));
            Assert.IsTrue(museAtRockInRio.TimeSlices.Any(ts => ts.Child.Name == "Patricia"));
            var ironMaidenAtRockInRio = ((response as OkObjectResult).Value as List<Concert>).Single(c => c.CommonParent.Name == "Iron Maiden");
            Assert.AreEqual(1, ironMaidenAtRockInRio.TimeSlices.Count);
            Assert.IsTrue(museAtRockInRio.TimeSlices.Any(ts => ts.Child.Name == "Joao"));
            Assert.AreEqual(museAtRockInRio.TimeSlices.Single(ts => ts.Child.Name == "Joao").Child, ironMaidenAtRockInRio.TimeSlices.Single(ts => ts.Child.Name == "Joao").Child);
        }
    }
}