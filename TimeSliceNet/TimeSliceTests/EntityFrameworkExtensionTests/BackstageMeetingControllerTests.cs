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
    public class BackstageMeetingControllerTests : ControllerTest
    {
        public BackstageMeetingControllerTests()
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
            var controller = new BackstageMeetingController(context);
            // test the concerts
            var response = await controller.GetAllBackstageMeetings();
            Assert.IsInstanceOf<OkObjectResult>(response);
            Assert.AreEqual(1, ((response as OkObjectResult).Value as List<BackstageMeetings>).Count);
            var museBackstage = ((response as OkObjectResult).Value as List<BackstageMeetings>).Single();
            Assert.AreEqual(2, museBackstage.TimeSlices.Count);
            Assert.IsTrue(museBackstage.TimeSlices.Any(ts => ts.Child.Name == "Joao"));
            Assert.IsTrue(museBackstage.TimeSlices.Any(ts => ts.Child.Name == "Patricia"));
            Assert.IsTrue(museBackstage.IsValid());
        }

        [Test]
        [NonParallelizable]
        public async Task TestSameBaseTypeEntriesMustNotInterfere()
        {
            await using var context = new TimeSliceContext(ContextOptions);
            await Seed(context);
            var joaoBackstage = context.BackstageMeetings.Single().TimeSlices.Single(ts => ts.Child.Name == "Joao");
            var joaoAtTheStage = context.Concerts.Single(c => c.Location == "rio" && c.CommonParent.Name == "Muse").TimeSlices.Single(ts => ts.Child.Name == "Joao");
            Assert.AreNotEqual(joaoBackstage.Discriminator, joaoAtTheStage.Discriminator);
        }
    }
}