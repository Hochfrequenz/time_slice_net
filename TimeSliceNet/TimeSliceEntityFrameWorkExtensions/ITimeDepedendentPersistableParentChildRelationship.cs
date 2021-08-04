using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     A persistable time dependent parent child relation is similar to a <see cref="TimeDependentRelation{TParent,TChild}" /> but has the additional constraint,
    ///     that the parent and child involved each have a key property, that can be used as a primary key on a database.
    /// </summary>
    /// <typeparam name="TPersistableParent">
    ///     <see cref="IRelation{TParent,TChild}.Parent" />
    /// </typeparam>
    /// <typeparam name="TParentKey"><see cref="IRelation{TParent,TChild}.Parent" />s key</typeparam>
    /// <typeparam name="TPersistableChild">
    ///     <see cref="IRelation{TParent,TChild}.Child" />
    /// </typeparam>
    /// <typeparam name="TChildKey"><see cref="IRelation{TParent,TChild}.Child" />s key</typeparam>
    public class
        PersistableTimeDependentRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey> :
            TimeDependentRelation<TPersistableParent, TPersistableChild>,
            IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>
        where TPersistableParent : class, IHasKey<TParentKey>
        where TPersistableChild : class, IHasKey<TChildKey>
    {
        public TParentKey ParentId { get; set; }
        public TChildKey ChildId { get; set; }
    }
}