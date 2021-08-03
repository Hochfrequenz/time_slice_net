using System.Threading;
using System.Threading.Tasks;
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

        // example classes from musician <-> listener example
        public DbSet<Musician> Musicians { get; set; }
        public DbSet<Listener> Listeners { get; set; }

        // two db sets using the same base type must not interfere
        public DbSet<Concert> Concerts { get; set; }
        public DbSet<BackstageMeetings> BackstageMeetings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<Musician>().HasKey(m => m.Name);
            // modelBuilder.Entity<Listener>().HasKey(l => l.Name);


            modelBuilder.SetDefaultKeys<Concert, PersistableConcertVisit, PersistableMusician, string, PersistableListener, string>(
                concert => concert.Guid, // The key of a collection (Concert) has to be explicitly set, always
                relationshipKeyExpression: null //  but keys for ConcertVisit can be derived automatically (null)
            );
            modelBuilder.SetDefaultKeys<BackstageMeetings, PersistableOneOnOneWithAStart, PersistableMusician, string, PersistableListener, string>(
                backstageMeeting => backstageMeeting.Guid, // The key of a collection (BackstageMeetings) has to be explicitly set, always
                relationshipKeyExpression: oneOnOneWithAStar => oneOnOneWithAStar.MeetingGuid // it's possible to also explicitly set a key for the relationship (!=null)
            );
        }
    }
}