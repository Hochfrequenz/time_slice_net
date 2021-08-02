using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TimeSlice;

namespace ExampleClasses.Music
{
    /// <summary>
    ///     a musician is someone who makes music
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Musician
    {
        /// <summary>
        ///     name of the musician
        /// </summary>
        /// <example>Freddie Mercury</example>
        public string Name { get; set; }
    }

    /// <summary>
    ///     a listener is someone who listens to music, made e.g. by <see cref="Musician" />s
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Listener
    {
        /// <summary>
        ///     name of the listener
        /// </summary>
        /// <example>John Doe</example>
        public string Name { get; set; }
    }

    public enum ListeningType
    {
        /// <summary>
        ///     live in concert
        /// </summary>
        Live,

        /// <summary>
        ///     on a record (e.g. CD, Vinyl, Streaming...)
        /// </summary>
        Record
    }

    /// <summary>
    ///     That a <see cref="Listener" /> listens to music by a <see cref="Musician" /> is modelled as a Musician Listener Relationship
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ListeningExperience : TimeDependentParentChildRelationship<Musician, Listener>
    {
        // add properties as you like

        /// <summary>
        ///     just an example property
        /// </summary>
        public virtual ListeningType ListeningType { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ConcertVisit : ListeningExperience
    {
        public override ListeningType ListeningType => ListeningType.Live;
    }

    /// <summary>
    ///     multiple allocations that vary over time are modeled as a "collection".
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Concert : TimeDependentParentChildCollection<ListeningExperience, Musician, Listener>
    {
        public Concert(Musician artist, IEnumerable<ListeningExperience> experiences = null) : base(artist, experiences)
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
}