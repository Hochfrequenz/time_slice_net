using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TimeSlice;
using TimeSliceEntityFrameworkExtensions;

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
    /// The persistable Musician is a <see cref="Musician"/>, that has a key on a database (hence implements <see cref="IHasKey{TKey}"/>.
    /// <seealso cref="PersistableListener"/>
    /// </summary>
    /// <remarks>Your classes only need to implement <see cref="IHasKey{TKey}"/> if you want to store them in a database, otherwise the plain <see cref="Musician"/> would be fine</remarks>
    public class PersistableMusician : Musician, IHasKey<string>
    {
        public string Key => Name;
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

    /// <summary>
    /// A persisitable Listener is a <see cref="Listener"/>, that has a key on a database (hence implements <see cref="IHasKey{TKey}"/>.
    /// <seealso cref="PersistableMusician"/>
    /// </summary>
    public class PersistableListener : Listener, IHasKey<string>
    {
        public string Key => Name;
    }

    /// <summary>
    ///     a fan (<see cref="Listener" />) can visit a concert of a <see cref="Musician" />
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConcertVisit : TimeDependentParentChildRelationship<Musician, Listener>
    {
    }

    /// <summary>
    /// A persistable concert visit is a <see cref="ConcertVisit"/> that can be stored in a database.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PersistableConcertVisit : ConcertVisit, IPersistableParentChildRelationship<PersistableMusician, string, PersistableListener, string>
    {
        public bool Equals(IParentChildRelationship<PersistableMusician, PersistableListener>? other)
        {
            throw new NotImplementedException();
        }

        public new PersistableMusician Parent { get => base.Parent as PersistableMusician; }
        public new PersistableListener Child { get => base.Child as PersistableListener; }
        public string ParentId { get; set; }
        public string ChildId { get; set; }
    }

    /// <summary>
    ///     a fan (<see cref="Listener" />) can meet a <see cref="Musician" /> backstage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OneOnOneWithAStar : TimeDependentParentChildRelationship<Musician, Listener>
    {
        public Guid MeetingGuid { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PersistableOneOnOneWithAStart : OneOnOneWithAStar, IPersistableParentChildRelationship<PersistableMusician, string, PersistableListener, string>
    {
        public bool Equals(IParentChildRelationship<PersistableMusician, PersistableListener>? other)
        {
            throw new NotImplementedException();
        }

        public string ParentId { get; set; }
        public string ChildId { get; set; }

        public new PersistableMusician Parent
        {
            get => base.Parent as PersistableMusician;
        }

        public new PersistableListener Child
        {
            get => base.Child as PersistableListener;
        }
    }

    /// <summary>
    ///     multiple allocations that vary over time are modeled as a "collection".
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Concert : TimeDependentParentChildCollection<ConcertVisit, Musician, Listener>
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
    public class BackstageMeetings : TimeDependentParentChildCollection<OneOnOneWithAStar, Musician, Listener>
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