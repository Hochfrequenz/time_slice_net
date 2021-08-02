using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using ExampleClasses.Music;
using ExampleWebApplication;
using ExampleWebApplication.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;

namespace ExampleWebApplicationTests
{
    public class ConcertControllerTests : ControllerTest, IDisposable
    {
        private readonly DbConnection _connection;

        public ConcertControllerTests()
            : base(
                new DbContextOptionsBuilder<TimeSliceContext>()
                    .UseSqlite(CreateInMemoryDatabase())
                    .Options)
        {
            _connection = RelationalOptionsExtension.Extract(ContextOptions).Connection;
        }

        void IDisposable.Dispose()
        {
            _connection.Dispose();
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        [Test]
        public async Task TestGetAll()
        {
            var muse = new Musician
            {
                Name = "Muse"
            };
            var joao = new Listener
            {
                Name = "Joao"
            };
            var patricia = new Listener
            {
                Name = "Patricia"
            };
            await using var context = new TimeSliceContext(ContextOptions);
            await context.Concerts.AddRangeAsync(
                new Concert
                {
                    Location = "new york"
                },
                new Concert
                {
                    Location = "rio",
                    CommonParent = muse,
                    TimeSlices =
                    {
                        new ConcertVisit
                        {
                            Child = joao,
                            Parent = muse,
                            Start = new DateTimeOffset(2013, 9, 14, 18, 0, 0, TimeSpan.FromHours(-5)),
                            End = new DateTimeOffset(2013, 9, 14, 21, 0, 0, TimeSpan.FromHours(-5))
                        },
                        new ConcertVisit
                        {
                            Child = patricia,
                            Parent = muse,
                            Start = new DateTimeOffset(2013, 9, 14, 17, 50, 0, TimeSpan.FromHours(-5)),
                            End = new DateTimeOffset(2013, 9, 14, 21, 0, 0, TimeSpan.FromHours(-5))
                        }
                    }
                },
                new Concert
                {
                    Location = "tokyo"
                });
            await context.SaveChangesAsync();
            var controller = new ConcertController(context);
            var response = await controller.GetAllConcerts("rio");
            Assert.IsInstanceOf<OkObjectResult>(response);
            Assert.AreEqual(1, ((response as OkObjectResult).Value as List<Concert>).Count);
            var rockInRio = ((response as OkObjectResult).Value as List<Concert>).Single();
            Assert.AreEqual(2, rockInRio.TimeSlices.Count);
            Assert.IsTrue(rockInRio.TimeSlices.Any(ts=>ts.Child.Name=="Joao"));
            Assert.IsTrue(rockInRio.TimeSlices.Any(ts=>ts.Child.Name=="Patricia"));
        }
    }
}