using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TimeSlice;
using TimeSliceEntityFrameworkExtensions;

namespace ExampleClasses.Festival
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
    /// A stage is a place where a <see cref="Musician"/> plays a <see cref="Concert"/>.
    /// At one stage there can be max. 1 concert at a time.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Stage : IHasKey<int>
    {
        /// <summary>
        /// unique number of this stage
        /// </summary>
        public int StageNumber { get; set; }

        /// <summary>
        /// The <see cref="StageNumber"/> of a stage is also its primary key on the database. <inheritdoc cref="IHasKey{TKey}.Id"/>
        /// </summary>
        int IHasKey<int>.Id
        {
            get => StageNumber;
            set { }
        }
    }

    /// <summary>
    /// A festival act is a musician playing at a stage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FestivalAct : PersistableTimeDependentRelation<Stage, int, Musician, string>
    {
    }

    /// <summary>
    /// A festival is a collection of <see cref="FestivalAct"/>s. At each Stage there can be max. 1 act at a time.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Festival : PersistableTimeDependentCollection<FestivalAct, Stage, int, Musician, string>
    {
        public override TimeDependentCollectionType CollectionType => TimeDependentCollectionType.PreventOverlaps;
    }


    /// <summary>
    ///     a fan (<see cref="Listener" />) can meet a <see cref="Musician" /> in private/backstage.
    ///     Other than at a <see cref="Concert"/> there a musician can only meet one <see cref="Listener"/> at a time in a OneOnONeWithAStar.
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
    ///     A musician can meet one listener at a time in a backstage meeting.
    ///     Each of those backstage meetings is a <see cref="OneOnOneWithAStar"/>.
    /// </summary>
    /// <remarks>The reason for this class is to have a <see cref="Musician"/> &lt;-&gt;<see cref="Listener"/> relation that is 1:[0 or 1] other than the 1:n <see cref="Concert"/></remarks>
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