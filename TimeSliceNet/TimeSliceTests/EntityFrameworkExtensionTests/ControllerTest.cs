using System;
using System.Data.Common;
using System.Threading.Tasks;
using ExampleClasses.Music;
using ExampleWebApplication;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TimeSliceTests.EntityFrameworkExtensionTests
{
    public abstract class ControllerTest : IDisposable
    {
        protected readonly DbConnection _connection;

        protected ControllerTest(DbContextOptions<TimeSliceContext> contextOptions)
        {
            ContextOptions = contextOptions;
            using var context = new TimeSliceContext(ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            _connection = RelationalOptionsExtension.Extract(ContextOptions).Connection;
        }

        protected DbContextOptions<TimeSliceContext> ContextOptions { get; }

        void IDisposable.Dispose()
        {
            _connection.Dispose();
        }

        protected static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        protected static async Task Seed(TimeSliceContext context)
        {
            var muse = new Musician
            {
                Name = "Muse"
            };
            var ironMaiden = new Musician
            {
                Name = "Iron Maiden"
            };
            await context.Musicians.AddRangeAsync(muse, ironMaiden);
            var carlos = new Listener
            {
                Name = "Carlos"
            };
            var joao = new Listener
            {
                Name = "Joao"
            };
            var patricia = new Listener
            {
                Name = "Patricia"
            };
            await context.Listeners.AddRangeAsync(joao, patricia);
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
                            Start = new DateTimeOffset(2013, 9, 14, 18, 0, 0, TimeSpan.FromHours(-3)),
                            End = new DateTimeOffset(2013, 9, 14, 21, 0, 0, TimeSpan.FromHours(-3))
                        },
                        new ConcertVisit
                        {
                            Child = carlos, // carlos arrives and leaves at the same time at the same concert as joao
                            Parent = muse, // this is basically a test that the primary key of the Concert Visit must also include the child (carlos/joao) key
                            // if this was not the case, there's be a iss
                            Start = new DateTimeOffset(2013, 9, 14, 18, 0, 0, TimeSpan.FromHours(-3)),
                            End = new DateTimeOffset(2013, 9, 14, 21, 0, 0, TimeSpan.FromHours(-3))
                        },
                        new ConcertVisit
                        {
                            Child = patricia,
                            Parent = muse,
                            Start = new DateTimeOffset(2013, 9, 14, 17, 50, 0, TimeSpan.FromHours(-3)),
                            End = new DateTimeOffset(2013, 9, 14, 21, 0, 0, TimeSpan.FromHours(-3))
                        }
                    }
                },
                new Concert
                {
                    Location = "rio",
                    CommonParent = ironMaiden,
                    TimeSlices =
                    {
                        new ConcertVisit
                        {
                            Child = joao,
                            Parent = muse,
                            Start = new DateTimeOffset(2013, 9, 14, 16, 0, 0, TimeSpan.FromHours(-3)),
                            End = new DateTimeOffset(2013, 9, 14, 17, 0, 0, TimeSpan.FromHours(-3))
                        }
                    }
                },
                new Concert
                {
                    Location = "tokyo"
                });
            await context.BackstageMeetings.AddRangeAsync(
                new BackstageMeetings
                {
                    CommonParent = muse,
                    TimeSlices =
                    {
                        new OneOnOneWithAStar
                        {
                            Child = joao,
                            Parent = muse,
                            Start = new DateTimeOffset(2013, 9, 14, 21, 30, 0, TimeSpan.FromHours(-3)),
                            End = new DateTimeOffset(2013, 9, 14, 22, 0, 0, TimeSpan.FromHours(-3))
                        },
                        new OneOnOneWithAStar
                        {
                            Child = patricia,
                            Parent = muse,
                            Start = new DateTimeOffset(2013, 9, 14, 22, 0, 0, TimeSpan.FromHours(-3)),
                            End = new DateTimeOffset(2013, 9, 14, 22, 30, 0, TimeSpan.FromHours(-3))
                        }
                    }
                });
            await context.SaveChangesAsync();
        }
    }
}