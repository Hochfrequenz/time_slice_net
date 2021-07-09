# time_slice_net
![Unittests Status Badge](https://github.com/Hochfrequenz/time_slice_net/workflows/Unittests%20and%20Coverage/badge.svg)
![Linter Status Badge](https://github.com/Hochfrequenz/time_slice_net/workflows/ReSharper/badge.svg)
![Formatter Status Badge](https://github.com/Hochfrequenz/time_slice_net/workflows/dotnet-format/badge.svg)

TimeSlice.NET is a C# based .NET package to model time slices ("𝘡𝘦𝘪𝘵𝘴𝘤𝘩𝘦𝘪𝘣𝘦𝘯") in an easily serializable and persistable way. We don't ignore time zones.

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
