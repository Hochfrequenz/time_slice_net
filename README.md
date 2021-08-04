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

| Simple Interface/ Base Class | Interface/Base Class to Persist using EF Core                                 |
| ---------------------------- | ----------------------------------------------------------------------------- |
| _no constraints_             | Parents and Childs used in relations have to implement `IHasKey<TPrimaryKey>` |
| `IRelation`                  | `IPersistableRelation`                                                        |
| `TimeDependentRelation`      | `PersistableTimeDependentReleation`                                           |
| `TimeDependentCollection`    | `PersistableTimeDependentCollection`                                          |

The generics used may look a bit overcomplicated to simply define a primary key (which you can "normally" do by using the `[Key]` attribute) but the real advantage is, that all the primary and foreign key relations for the collection are then automatically set up using

```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.SetupCollectionAndRelations<MyCollectionType, MyRelationType, MyParent, MyParentsKey, MyChild, MyChildsKey>(collection=>collection.YourKey);
    // that's all.
}
```

See the [ExampleWebApplication 🠒 TimeSliceContext](TimeSliceNet/ExampleWebApplication/TimeSliceContext.cs) class for a working (and [unit tested](TimeSliceNet/TimeSliceTests/EntityFrameworkExtensionTests)) example.
