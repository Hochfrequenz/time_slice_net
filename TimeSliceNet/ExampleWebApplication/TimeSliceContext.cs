using ExampleClasses.Music;
using Microsoft.EntityFrameworkCore;
using TimeSliceEntityFrameworkExtensions;

namespace ExampleWebApplication
{
    /// <summary>
    ///     a database contexts that uses the <see cref="ExampleClasses" />
    /// </summary>
    public class TimeSliceContext : DbContext
    {
        public TimeSliceContext(DbContextOptions<TimeSliceContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Musician> Musicians { get; set; }

        public DbSet<Listener> Listeners { get; set; }

        // two db sets using the same base type must not interfere
        public DbSet<Concert> Concerts { get; set; }
        public DbSet<BackstageMeetings> BackstageMeetings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetupCollectionAndRelations<Concert, ConcertVisit, Musician, string, Listener, string>(
                concert => concert.Guid //  but keys for ConcertVisit can be derived automatically (null)
            );
            modelBuilder.SetupCollectionAndRelations<BackstageMeetings, OneOnOneWithAStar, Musician, string, Listener, string>(
                backstageMeeting => backstageMeeting.Guid, // The key of a collection (BackstageMeetings) has to be explicitly set, always
                oneOnOneWithAStar => oneOnOneWithAStar.MeetingGuid // it's possible to also explicitly set a key for the relation (!=null)
            );
        }
    }
}