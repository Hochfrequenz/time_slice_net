using ExampleWebApplication;
using Microsoft.EntityFrameworkCore;

namespace TimeSliceTests.EntityFrameworkExtensionTests
{
    public abstract class ControllerTest
    {
        protected ControllerTest(DbContextOptions<TimeSliceContext> contextOptions)
        {
            ContextOptions = contextOptions;
            using var context = new TimeSliceContext(ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        protected DbContextOptions<TimeSliceContext> ContextOptions { get; }
    }
}