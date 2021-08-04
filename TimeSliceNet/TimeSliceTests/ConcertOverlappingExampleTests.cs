using System;
using System.Linq;
using ExampleClasses.Music;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Test cases that are not thought to cover every edge case but rather demonstrate the main ideas of this library.
    ///     In this test class we cover <see cref="TimeDependentCollection{TRelation,TParent,TChild}" />s.
    /// </summary>
    /// <remarks>To understand these tests, first have a look at <seealso cref="MusicianListenerRelationExampleTests" /></remarks>
    public class ConcertOverlappingExampleTests
    {
        /// <summary>
        ///     demonstrates the use of the <see cref="TimeDependentCollection{TRelation,TParent,TChild}" /> with <see cref="TimeDependentCollectionType.AllowOverlaps" />
        /// </summary>
        [Test]
        public void TestMultipleListenersAtTheSameConcert()
        {
            // The listener and musician class are imported from the ExampleClasses project -> Concert namespace
            var alice = new Listener();
            var bob = new Listener();
            var freddyMercury = new Musician();
            // if alice listens to freddy mercury at a concert it looks like this
            var aliceAtWembley = new ConcertVisit
            {
                Start = new DateTimeOffset(1986, 7, 12, 19, 0, 0, TimeSpan.Zero), // alice just entered wembley stadium when the concert began
                End = new DateTimeOffset(1986, 7, 13, 0, 0, 0, TimeSpan.Zero),
                Parent = freddyMercury,
                Child = alice
            };
            var bobAtWembley = new ConcertVisit
            {
                Start = new DateTimeOffset(1986, 7, 12, 17, 0, 0, TimeSpan.Zero), // bob arrived super early
                End = new DateTimeOffset(1986, 7, 12, 23, 0, 0, TimeSpan.Zero), // but had to leave early
                Parent = freddyMercury, // same musician
                Child = bob
            };
            // collections are initialized by providing the common parent (the "1" in the 1:n cardinality)
            var liveAtWembley = new Concert(freddyMercury, new[] { aliceAtWembley, bobAtWembley });

            //   17   18  19  20  21  22  23  24     (time) ---->
            // ...|...|...|...|...|...|...|...|..
            //   [ bob listens to freddy  )
            //            [ alice at concert  )
            //            [--overlap ok---)
            Assert.IsTrue(liveAtWembley.IsValid()); // collections, that allow overlaps are valid iff each of the single items is valid
            // it's easier to model and validate than collections that are exclusive

            var peopleThatCouldHaveMetAtWembley =
                from attendanceA in liveAtWembley.TimeSliceList
                from attendanceB in liveAtWembley.TimeSliceList
                where !ReferenceEquals(attendanceA, attendanceB)
                      && attendanceA.Overlaps(attendanceB)
                select (attendanceA.Child, attendanceB.Child);
            Assert.Contains((alice, bob), peopleThatCouldHaveMetAtWembley.ToList());
        }
    }
}