using ExampleClasses.Music;
using Microsoft.EntityFrameworkCore;
using TimeSliceEntityFrameWorkExtensions;

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
        public DbSet<ListeningExperience> ListeningExperiences { get; set; }

        // two db sets using the same base type must not interfere
        public DbSet<Concert> Concerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Musician>().HasKey(m => m.Name);
            modelBuilder.Entity<Listener>().HasKey(l => l.Name);

            modelBuilder.Entity<ListeningExperience>().HasDefaultKeys<ListeningExperience, Musician, Listener>(le => le.Guid);
            modelBuilder.Entity<Concert>().HasDefaultKeys<Concert, ListeningExperience, Musician, Listener>(c => c.Guid);
            modelBuilder.Entity<Concert>().ToTable("Concerts");
        }
    }
}