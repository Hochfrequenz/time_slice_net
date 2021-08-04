using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     Similar to <see cref="TimeDependentParentChildCollection{TRelation,TParent,TChild}" /> but with persistable parent and child
    /// </summary>
    /// <typeparam name="TPersistableRelation"></typeparam>
    /// <typeparam name="TPersistableParent"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    /// <typeparam name="TPersistableChild"></typeparam>
    /// <typeparam name="TChildKey"></typeparam>
    public abstract class PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey> :
        TimeDependentParentChildCollection<TPersistableRelation, TPersistableParent, TPersistableChild>,
        IEquatable<PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>>
        where TPersistableRelation : IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>, ITimeSlice, IValidatableObject,
        IRelation<TPersistableParent, TPersistableChild>
        where TPersistableParent : class, IHasKey<TParentKey>
        where TPersistableChild : class, IHasKey<TChildKey>
    {
        protected PersistableTimeDependentCollection(TPersistableParent commonParent, IEnumerable<TPersistableRelation> relations = null)
            : base(commonParent, relations)
        {
        }

        protected PersistableTimeDependentCollection()
        {
        }

        public bool Equals(PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey> other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() &&
                   Equals((PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}