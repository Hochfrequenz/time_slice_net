using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     Extensions to <see cref="ModelBuilder"/>
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Set default keys using <see cref="EntityTypeBuilderExtensions.HasDefaultKeys{TTimeSliceCollection,TRelationship,TParent,TChild}"/>.
        /// </summary>
        /// <param name="modelBuilder">a model builder instance</param>
        /// <param name="collectionKeyExpression">each collection has to have a key defined</param>
        /// <param name="relationshipKeyExpression">optional key for each <typeparamref name="TRelationship"/></param>
        /// <typeparam name="TTimeSliceCollection"><see cref="TimeDependentParentChildCollection{TRelationship,TParent,TChild}"/></typeparam>
        /// <typeparam name="TRelationship"><see cref="TimeDependentParentChildRelationship{TParent,TChild}"/></typeparam>
        /// <typeparam name="TParent"><see cref="TimeDependentParentChildRelationship{TParent,TChild}.Parent"/></typeparam>
        /// <typeparam name="TChild"><see cref="TimeDependentParentChildRelationship{TParent,TChild}.Child"/></typeparam>
        /// <typeparam name="TParentKey"><see cref="IHasKey{TKey}.Key"/> of <typeparamref name="TParent"/></typeparam>
        /// <typeparam name="TChildKey"><see cref="IHasKey{TKey}.Key"/> of <typeparamref name="TChild"/></typeparam>
        public static void SetDefaultKeys<TTimeSliceCollection, TRelationship, TParent, TParentKey, TChild, TChildKey>(this ModelBuilder modelBuilder,
            [NotNull] Expression<Func<TTimeSliceCollection, object>> collectionKeyExpression,
            Expression<Func<TRelationship, object>> relationshipKeyExpression = null)
            where TTimeSliceCollection : TimeDependentParentChildCollection<TRelationship, TParent, TChild>
            where TRelationship : class, ITimeSlice, IPersistableParentChildRelationship<TParent, TParentKey, TChild, TChildKey>
            where TParent : class, IHasKey<TParentKey>
            where TChild : class, IHasKey<TChildKey>
        {
            modelBuilder.Entity<TParent>().HasKey(parent => parent.Key);
            modelBuilder.Entity<TChild>().HasKey(child => child.Key);
            modelBuilder.Entity<TTimeSliceCollection>().HasDefaultKeys<TTimeSliceCollection, TRelationship, TParent, TChild>(collectionKeyExpression);
            modelBuilder.Entity<TRelationship>().HasKey(x => new { x.Discriminator, x.ParentId, x.ChildId, x.Start, x.End });
            modelBuilder.Entity<TRelationship>().HasDefaultKeys<TRelationship, TParent, TChild>(relationshipKeyExpression);
        }
    }
}