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
- All end date times are, if set to a finite value other than `MaxValue`, meant and treated as exclusive.

## Code Quality / Production Readiness

The code has [at least a 95%](https://github.com/Hochfrequenz/time_slice_net/blob/main/.github/workflows/unittests_and_coverage.yml#L34) unit test coverage.
So if you trust the tests, you can probably also trust the code.

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

### Relationships

Relationships describe that "parents" have "children" assigned for a specific time range.
For a minimal, easy to understand example on relationships, see the [parking lot ⬌ car relation ship tests](TimeSliceNet/TimeSliceTests/ParkingLotCarRelationshipExampleTests.cs).

## Relationships that vary over time
In many business cases these relations vary over time; children are assigned and unassigned at specific points in time.
We call these assignments "time dependent parent child relationship collection".
There are two main kinds:
* overlaps are allowed (any number of children per point in time)
* overlaps are forbidden (max. 1 child per point in time)

```c#

```