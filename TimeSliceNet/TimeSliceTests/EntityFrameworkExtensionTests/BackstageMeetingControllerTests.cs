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
            using (ContextIsInUseSemaphore)
            {
                var controller = new BackstageMeetingController(context);
                // test the concerts
                var response = await controller.GetAllBackstageMeetings();
                response.Should().BeOfType<OkObjectResult>();
                (response as OkObjectResult).Value.Should().BeOfType<List<BackstageMeetings>>();
                ((response as OkObjectResult).Value as List<BackstageMeetings>).Should().HaveCount(1);
                var museBackstage = ((response as OkObjectResult).Value as List<BackstageMeetings>).Single();
                museBackstage.TimeSlices.Should().HaveCount(2);
                museBackstage.CommonParentId.Should().BeEquivalentTo("Muse"); // CommonParentId has to be set automatically by DefaultValueGenerator
                museBackstage.TimeSlices.Any(ts => ts.Child.Name == "Joao").Should().BeTrue();
                museBackstage.TimeSlices.Any(ts => ts.Child.Name == "Patricia").Should().BeTrue();
                museBackstage.IsValid().Should().BeTrue();
            }
        }

        [Test]
        [NonParallelizable]
        public void TestSameBaseTypeEntriesMustNotInterfere()
        {
            using (ContextIsInUseSemaphore)
            {
                var joaoBackstage = context.BackstageMeetings.Single().TimeSlices.Single(ts => ts.Child.Name == "Joao");
                joaoBackstage.ChildId.Should().BeEquivalentTo("Joao"); // ChildId has to be set automatically by DefaultValueGenerator
                joaoBackstage.ParentId.Should().BeEquivalentTo("Muse"); // ParentId has to be set automatically by DefaultValueGenerator
                var joaoAtTheStage = context.Concerts.Single(c => c.Location == "rio" && c.CommonParent.Name == "Muse").TimeSlices.Single(ts => ts.Child.Name == "Joao");
                joaoAtTheStage.Should().NotBe(joaoBackstage.Discriminator);
            }
        }
    }
}
