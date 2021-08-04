using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TimeSlice;
using TimeSliceEntityFrameworkExtensions;

namespace ExampleClasses.Music
{
    // The example classes from this name space are a bit more complex than those from the gas pump/gas station example.
    // This is because each of the classes defined here is persistable using Entity Framework Core.
    // For this to be the case, they all have to define a primary key which is done using Generics (instead of attributes), so that in the end there is one generic method,
    // that can set up the relations and constraints on the database.

    /// <summary>
    ///     a musician is someone who makes music
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Musician : IHasKey<string>
    {
        /// <summary>
        ///     name of the musician
        /// </summary>
        /// <example>Freddie Mercury</example>
        public string Name { get; set; }

        /// <summary>
        ///     The <see cref="Name" /> is also the primary key in the musician table on the database. <inheritdoc cref="IHasKey{TKey}.Id" />
        /// </summary>
        string IHasKey<string>.Id
        {
            get => Name;
            set { }
        }
    }


    /// <summary>
    ///     a listener is someone who listens to music, made e.g. by <see cref="Musician" />s
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Listener : IHasKey<string>
    {
        /// <summary>
        ///     name of the listener
        /// </summary>
        /// <example>John Doe</example>
        public string Name { get; set; }

        /// <summary>
        ///     The <see cref="Name" /> is also the primary key of the listeners table on the database. <inheritdoc cref="IHasKey{TKey}.Id" />
        /// </summary>
        string IHasKey<string>.Id
        {
            get => Name;
            set { }
        }
    }

    /// <summary>
    ///     A <see cref="Listener" /> can visit a concert of a <see cref="Musician" />.
    ///     The time slice the listener spends at the concert is modelled as Concert Visit.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConcertVisit : PersistableTimeDependentRelation<Musician, string, Listener, string>
    {
    }

    /// <summary>
    ///     a fan (<see cref="Listener" />) can meet a <see cref="Musician" /> in private/backstage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OneOnOneWithAStar : PersistableTimeDependentRelation<Musician, string, Listener, string>
    {
        /// <summary>
        ///     it's possible but not necessary to define ones own key
        /// </summary>
        public Guid MeetingGuid { get; set; }
    }

    /// <summary>
    ///     multiple allocations that vary over time are modeled as a "collection".
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Concert : PersistableTimeDependentCollection<ConcertVisit, Musician, string, Listener, string>
    {
        public Concert(Musician artist, IEnumerable<ConcertVisit> experiences = null) : base(artist, experiences)
        {
        }

        public Concert()
        {
        }

        /// <summary>
        ///     unique ID of this concert
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     location where the concert takes place
        /// </summary>
        /// <example>Rio, Wembley, Berlin</example>
        public string Location { get; set; }

        /// <summary>
        ///     At a concert multiple listeners can enjoy the same musician at the same time
        /// </summary>
        public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.AllowOverlaps;
    }

    /// <summary>
    ///     A musician can meet one listener at a time in a backstage meeting.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BackstageMeetings : PersistableTimeDependentCollection<OneOnOneWithAStar, Musician, string, Listener, string>
    {
        public BackstageMeetings(Musician artist, IEnumerable<OneOnOneWithAStar> experiences = null) : base(artist, experiences)
        {
        }

        public BackstageMeetings()
        {
        }

        /// <summary>
        ///     unique ID of this backstage meeting
        /// </summary>
        public Guid Guid { get; set; }


        /// <summary>
        ///     At a concert multiple listeners can enjoy the same musician at the same time
        /// </summary>
        public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.PreventOverlaps;
    }
}