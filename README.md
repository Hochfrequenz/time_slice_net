# time_slice_net

![Unittests Status Badge](https://github.com/Hochfrequenz/time_slice_net/workflows/Unittests%20and%20Coverage/badge.svg)
![Linter Status Badge](https://github.com/Hochfrequenz/time_slice_net/workflows/ReSharper/badge.svg)
![Formatter Status Badge](https://github.com/Hochfrequenz/time_slice_net/workflows/dotnet-format/badge.svg)

TimeSlice.NET is a C# based .NET package to model time slices ("𝘡𝘦𝘪𝘵𝘴𝘤𝘩𝘦𝘪𝘣𝘦𝘯") in an easily serializable and persistable way.
We know and appreciate the [Itenso TimePeriodLibrary](https://github.com/Giannoudis/TimePeriodLibrary) library which is great f.e. to calculate overlaps and intersections of multiple time periods.
However the focus of TimeSlice.NET is slightly different:

- TimeSlice.NET focuses on modeling time-dependent relationships between entities (think of ownership or assignments).
- TimeSlice.NET brings easy to use serialization (`System.Text.Json`) and persistence (Entity Framework) features for said time-dependent relationships.
- However TimeSlice.NET does not support as many different kinds of time periods or time period chains/ranges/collections as Itenso TimePeriodLibrary.

Furthermore:

- The code is designed with time zones in mind. They exist and will cause problems if ignored.
- All end date times are, if set to a finite value other than `MaxValue`, meant and treated as exclusive ([here's why](https://hf-kklein.github.io/exclusive_end_dates.github.io/))
- All sub second times (milliseconds and ticks) are ignored because they tend to cause trouble on the database/ORM level and there's barely a business case that requires them

## Release Workflow
To create a **pre-release** nuget package, create a tag of the form `prerelease-vx.y.z` where `x.y.z` is the semantic version of the pre-release.

## Code Quality / Production Readiness

- The code has [at least a 95%](https://github.com/Hochfrequenz/time_slice_net/blob/main/.github/workflows/unittests_and_coverage.yml#L34) unit test coverage. ✔️
- The bare TimeSlice.NET package has no extra dependencies. ✔️
- The only dependency of the TimeSlice.NET Entity Framework Extensions package is EF Core itself. ✔️

## Examples

### Plain Time Slices

The easiest way to think of a time slice is something that just has a start and (maybe also) an end.

```c#
var plainTimeSlice = new PlainTimeSlice
{
    Start = DateTimeOffset.UtcNow,
    End = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)
}
```

### "Open" Time Slices

Time slices are called "open", if their end is either not set or infinity.

```c#
var openTimeSlice = new PlainTimeSlice
{
    Start = DateTimeOffset.UtcNow,
    End = null
};
Assert.IsTrue(openTimeSlice.IsOpen()); // no end set => "open"
openTimeSlice.End = DateTimeOffset.MaxValue;
Assert.IsTrue(openTimeSlice.IsOpen()); // end is infinity => "open"
```

## Relations

A relation describes that a single "parent" has a single "child" assigned for a specific time range.
For a minimal, easy to understand example on relations, see the [gasoline pump ⬌ car relation tests](TimeSliceNet/TimeSliceTests/GasolinePumpCarRelationExampleTests.cs).

### Relations that vary over time = Collections

In many business cases these relations vary over time; children are assigned and unassigned to/from parents at specific points in time.
We call these assignments "time dependent collection".
There are two main kinds:

- overlaps are allowed = any number of children per point in time (easy to handle)
- overlaps are forbidden = max. 1 child per point in time (harder to handle)

For a minimal, easy to understand example of collections with overlapping children see the [concert tests](TimeSliceNet/TimeSliceTests/ConcertOverlappingExampleTests.cs).

For a minimal, easy to understand example of collections of non-overlapping children see the [gasoline pump ⬌ car (non overlapping) collection tests](TimeSliceNet/TimeSliceTests/GasolinePumpCarNonOverlappingExampleTests.cs).

## Storing the Collections on a Database using Entity Framework Core

In the [`TimeSliceEntityFrameworkExtensions`](TimeSliceNet/TimeSliceEntityFrameWorkExtensions) package you'll find extension classes that make your time slices, relations and collections of relations easily persistable using EF Core.

To make a relation persistable, simply change the interfaces known from above minimal working examples to:

| Simple Interface/ Base Class | Interface/Base Class to Persist using EF Core                                   |
| ---------------------------- | ------------------------------------------------------------------------------- |
| _no constraints_             | Parents and Children used in relations have to implement `IHasKey<TPrimaryKey>` |
| `IRelation`                  | `IPersistableRelation`                                                          |
| `TimeDependentRelation`      | `PersistableTimeDependentReleation`                                             |
| `TimeDependentCollection`    | `PersistableTimeDependentCollection`                                            |

The generics used may look a bit overcomplicated to simply define a primary key (which you can "normally" do by using the `[Key]` attribute) but the real advantage is, that all the primary and foreign key relations for the collection are then automatically set up using

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.SetupCollectionAndRelations<MyCollectionType, MyRelationType, MyParent, MyParentsKey, MyChild, MyChildsKey>(collection=>collection.YourKey);
    // that's all.
}
```

See the [ExampleWebApplication 🠒 TimeSliceContext](TimeSliceNet/ExampleWebApplication/TimeSliceContext.cs) class for a working (and [unit tested](TimeSliceNet/TimeSliceTests/EntityFrameworkExtensionTests)) example.

### From Scratch: Defining a Persistable Time Dependent 1:n and 1:1 Relations and Collections

There's a festival in town.
The persons attending the festival are either `Musician`s or `Listener`s.
For simplicity we model both of those types in separate, easily distinguishable classes.
Both `Musician` and `Listener` have a name that is, in both cases also used as primary key to store them on a database.

```c#
public class Musician : IHasKey<string>
{
    public string Name { get; set; } // e.g. Freddie Mercury
    string IHasKey<string>.Id => Name; // <-- used a PK in musician table
}

public class Listener : IHasKey<string>
{
    public string Name { get; set; } // e.g. John Doe
    string IHasKey<string>.Id => Name; // <-- used a PK in listener table
}
```

If a `Listener` attends a concert, this is modelled as a _relation_ where the `Musician` is a _parent_ to which the `Listener` is assigned as a _child_.

```c#
public class ConcertVisit : PersistableTimeDependentRelation<Musician, string, Listener, string>
{
    // no class body needed, everything we need is already inherited from the base class
}
```

At a concert there is usually _`1`_ `Musician` playing for _`n`_ listeners.
This 1:n cardinality explains why the type `Musician` is referred to as "_parent_" and the type `Listener` is referred to as "_child_".
In the names used in this library the "1" side of a cardinality is always named "parent".

The entire `Concert`, that consists of multiple n `Listener` listening to the same 1 `Musician` at (possibly but not necessarily) the same time is defined as a `Collection` of n `ConcertVisit`s.

```c#
public class Concert : PersistableTimeDependentCollection<ConcertVisit, Musician, string, Listener, string>
{
    // each collection has to define if the children involved in it
    // at a concert the visits of listeners may overlaps
    public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.AllowOverlaps;

    // the key of a collection is not enforced using generics, because it's not necessary.
    // so we could use anything else as a key but choosing a Guid is definitly not a bad idea at all.
    public Guid ConcertId { get; set; } // unique ID of the concert
}
```

To store concerts on a database we simply have to add one line to the `OnModelCreating` method:

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ...
    modelBuilder.SetupCollectionAndRelations<Concert, ConcertVisit, Musician, string, Listener, string>(concert=>concert.ConcertId);
    // This will set up:
    // * the Primary Keys for Musicians and Listeners
    // * the 1:n cardinality and unique constraints for the musician<->listener relation
    // * the table and keys for the concerts
}
```

Now we can filling the concert hall:

```c#
var freddy = new Musician { Name = "Freddie Mercury" };
var liveAtWembley = new Concert(freddy, new List<ConcertVisit>
{
    new()
    {
        Start = DateTimeOffset.Parse("1986-07-12T19:00:00+00:00"),
        End = DateTimeOffset.Parse("1986-07-12T22:00:00+00:00"),
        Child = new Listener { Name = "John Doe" };,
        Parent = freddy
    },
    new()
    {
        Start = DateTimeOffset.Parse("1986-07-12T19:30:00+00:00"),
        End = DateTimeOffset.Parse("1986-07-12T21:35:00+00:00"),
        Child = new Listener { Name = "Erika Musterfrau" },
        Parent = freddy
    }
    // ... many more
});
// add to context and save on database
await context.Concerts.AddAsync(liveAtWembley);
await context.SaveChangesAsync();
```
