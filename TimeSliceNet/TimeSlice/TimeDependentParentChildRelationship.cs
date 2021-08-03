using System;
using System.Collections.Generic;

namespace TimeSlice
{
    /// <summary>
    ///     The simplest implementation of a time dependent parent child relationship.
    /// </summary>
    public class TimeDependentParentChildRelationship<TParent, TChild> : PlainTimeSlice, IParentChildRelationship<TParent, TChild> where TParent : class where TChild : class

    {
        private string _discriminator;

        /// <inheritdoc />
        public bool Equals(IParentChildRelationship<TParent, TChild> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Discriminator == other.Discriminator && EqualityComparer<TParent>.Default.Equals(Parent, other.Parent) &&
                   EqualityComparer<TChild>.Default.Equals(Child, other.Child);
        }

        /// <inheritdoc />
        public string Discriminator
        {
            get => _discriminator ?? $"{GetType().FullName}";
            set
            {
                if (!string.IsNullOrWhiteSpace(value)) _discriminator = value;
            }
        }

        /// <summary>
        ///     The entity that "owns" / has assigned <see cref="Child" /> in between [<see cref="ITimeSlice.Start" /> and <see cref="ITimeSlice.End" />)
        /// </summary>
        public TParent Parent { get; set; }

        /// <summary>
        ///     The entity that is owned by / assigned to <see cref="Parent" /> in between [<see cref="ITimeSlice.Start" /> and <see cref="ITimeSlice.End" />)
        /// </summary>
        public TChild Child { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IParentChildRelationship<TParent, TChild>)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), _discriminator, Parent, Child);
        }
    }
}