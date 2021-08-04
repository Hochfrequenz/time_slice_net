using System;
using System.Data.Common;
using System.Threading;
using ExampleClasses.Music;
using ExampleWebApplication;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using NUnit.Framework;
using TimeSliceEntityFrameworkExtensions;

namespace TimeSliceTests.EntityFrameworkExtensionTests
{
    /// <summary>
    /// This class tests the ValueGenerators from <see cref="ModelBuilderExtensions"/>.
    /// The success cases are tested else where; the tests here focus on the exceptions (to get a better line coverage)
    /// </summary>
    public class ValueGeneratorsTests
    {
        protected DbContextOptions<TimeSliceContext> ContextOptions { get; }
        protected TimeSliceContext context;

        protected static readonly SemaphoreSlim ContextIsInUseSemaphore = new(1);

        protected static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        public ValueGeneratorsTests()
        {
            ContextOptions = new DbContextOptionsBuilder<TimeSliceContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .Options;
            context = new TimeSliceContext(ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        private EntityEntry GetMusicianEntry()
        {
            var musician = new Musician
            {
                Name = "Not a relation"
            };
            context.Attach(musician);
            return context.Entry(musician);
        }

        [Test]
        [NonParallelizable]
        public void ValueGeneratorsThrowNotImplementedExceptions()
        {
            using (ContextIsInUseSemaphore)
            {
                var childIdGenerator = new ChildIdValueGenerator<ConcertVisit, Musician, string, Listener, string>();
                var parentIdGenerator = new ParentIdValueGenerator<ConcertVisit, Musician, string, Listener, string>();
                var commonParentIdGenerator = new CommonParentIdValueGenerator<Concert, ConcertVisit, Musician, string, Listener, string>();
                EntityEntry entry = null;
                try
                {
                    entry = GetMusicianEntry(); // this entry is not usable for any of the value generators in place
                    Assert.Throws<NotImplementedException>(() => childIdGenerator.Next(entry));
                    Assert.Throws<NotImplementedException>(() => parentIdGenerator.Next(entry));
                    Assert.Throws<NotImplementedException>(() => commonParentIdGenerator.Next(entry));
                }
                finally
                {
                    if (entry != null)
                    {
                        entry.State = EntityState.Detached;
                    }
                }
            }
        }
    }
}