using System.Diagnostics.CodeAnalysis;
using TimeSlice;

namespace ExampleClasses.GasStation
{
    // The gas station examples are pretty simple/dummy classes.
    // Other than the ExampleClasses from the Music sub namespace they are not persistable on a database.

    /// <summary>
    ///     a car needs gasoline which it gets from a <see cref="GasolinePump" />
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Car
    {
    }

    /// <summary>
    ///     a gasoline pump can be used by only one <see cref="Car" /> at a time
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GasolinePump
    {
    }

    /// <summary>
    ///     The allocation/usage of a <see cref="GasolinePump" /> by a <see cref="Car" /> for a specific time frame is called "allocation".
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GasolinePumpAllocation : TimeDependentRelation<GasolinePump, Car>
    {
        // add properties as you like

        /// <summary>
        ///     just an example property
        /// </summary>
        public decimal LitersPumped { get; set; }
    }

    /// <summary>
    ///     multiple allocations that vary over time are modeled as a "collection".
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GasolinePumpAllocationCollection : TimeDependentCollection<GasolinePumpAllocation, GasolinePump, Car>
    {
        public GasolinePumpAllocationCollection(GasolinePump commonParent) : base(commonParent)
        {
        }

        public GasolinePumpAllocationCollection()
        {
        }

        /// <summary>
        ///     one gasoline station can only be used by one car at a time => temporal overlaps are not allowed.
        /// </summary>
        public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.PreventOverlaps;
    }
}