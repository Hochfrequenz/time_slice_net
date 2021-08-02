using System;

namespace TimeSlice
{
    /// <summary>
    ///     A parent child-relationship between one <typeparamref name="TParent" /> that owns / has assigned up to n <typeparamref name="TChild" />s
    ///     If combined with <see cref="ITimeSlice" /> this allows to model 1:n time dependent relationships.
    /// </summary>
    /// <typeparam name="TParent">the parent / owner type</typeparam>
    /// <typeparam name="TChild">the child / "owned" type</typeparam>
    public interface IParentChildRelationship<TParent, TChild> : IEquatable<IParentChildRelationship<TParent, TChild>> where TParent : class where TChild : class
    {
        /// <summary>
        ///     There might be more than one relation between <typeparamref name="TParent" /> and <typeparamref name="TChild" />.
        ///     The discriminator is a way to distinguish them.
        /// </summary>
        /// <remarks>This is thought to be used when persisting relationships on a database where the discriminator might be part of a Primary Key/Unique/Check Constraint</remarks>
        public string Discriminator { get; }

        /// <summary>
        ///     The entity that "owns" / has assigned <see cref="Child" />
        /// </summary>
        public TParent Parent { get; }

        /// <summary>
        ///     The entity that is owned by / assigned to <see cref="Parent" />
        /// </summary>
        public TChild Child { get; }
    }
}