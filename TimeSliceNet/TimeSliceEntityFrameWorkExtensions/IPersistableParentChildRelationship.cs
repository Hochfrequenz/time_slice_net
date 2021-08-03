using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    /// A persistable parent child relationship is similar to a <see cref="IParentChildRelationship{TParent,TChild}"/> but has the additional constraint,
    /// that the parent and child involved each have a key property, that can be used as a primary key on a database.
    /// </summary>
    /// <typeparam name="TPersistableParent"><see cref="IParentChildRelationship{TParent,TChild}.Parent"/></typeparam>
    /// <typeparam name="TParentKey"><see cref="IParentChildRelationship{TParent,TChild}.Parent"/>s key</typeparam>
    /// <typeparam name="TPersistableChild"><see cref="IParentChildRelationship{TParent,TChild}.Child"/></typeparam>
    /// <typeparam name="TChildKey"><see cref="IParentChildRelationship{TParent,TChild}.Child"/>s key</typeparam>
    public interface
        IPersistableParentChildRelationship<TPersistableParent, TParentKey, TPersistableChild, TChildKey> : IParentChildRelationship<TPersistableParent, TPersistableChild>
        where TPersistableParent : class, IHasKey<TParentKey>
        where TPersistableChild : class, IHasKey<TChildKey>
    {
        /// <summary>
        /// Unique ID of the <typeparamref name="TPersistableParent"/> involved in the relationship
        /// </summary>
        TParentKey ParentId { get; set; }

        /// <summary>
        /// Unique ID of the <typeparamref name="TPersistableChild"/> involved in the relationship
        /// </summary>
        TChildKey ChildId { get; set; }
    }
}