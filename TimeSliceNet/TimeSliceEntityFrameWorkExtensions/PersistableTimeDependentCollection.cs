using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     Similar to <see cref="TimeDependentCollection{TRelation,TParent,TChild}" /> but with persistable parent and child
    /// </summary>
    /// <typeparam name="TPersistableRelation"></typeparam>
    /// <typeparam name="TPersistableParent"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    /// <typeparam name="TPersistableChild"></typeparam>
    /// <typeparam name="TChildKey"></typeparam>
    public abstract class PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey> :
        TimeDependentCollection<TPersistableRelation, TPersistableParent, TPersistableChild>,
        IEquatable<PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>>
        where TPersistableRelation : IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>, ITimeSlice, IValidatableObject,
        IRelation<TPersistableParent, TPersistableChild>
        where TPersistableParent : class, IHasKey<TParentKey>
        where TPersistableChild : class, IHasKey<TChildKey>
    {
        /// <inheritdoc />
        protected PersistableTimeDependentCollection(TPersistableParent commonParent, IEnumerable<TPersistableRelation> relations = null)
            : base(commonParent, relations)
        {
        }

        /// <inheritdoc />
        protected PersistableTimeDependentCollection()
        {
        }

        /// <summary>
        ///     ID of <see cref="TimeDependentCollection{TRelation,TParent,TChild}.CommonParent" />
        /// </summary>
        public TParentKey CommonParentId { get; set; }

        /// <summary>
        ///     This property is mapped to a column on the database.
        ///     This allows us to enforce only valid collections on the database using a Check constraint.
        ///     The setter is only fake.
        /// </summary>
        public bool IsValid
        {
            get => this.IsValid();
            set { } // not necessary
        }

        /// <inheritdoc />
        public bool Equals(PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey> other)
        {
            return base.Equals(other) && other.CommonParentId.Equals(CommonParentId);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(CommonParentId, base.GetHashCode());
        }
    }
}