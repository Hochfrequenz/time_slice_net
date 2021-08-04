using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     Extensions to <see cref="ModelBuilder" />
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        ///     Set default keys using
        ///     <see cref="EntityTypeBuilderExtensions.HasDefaultKeys{TTimeSliceCollection,TPersistableRelation,TPersistableParent,TParentKey,TPersistableChild,TChildKey}" />.
        /// </summary>
        /// <param name="modelBuilder">a model builder instance</param>
        /// <param name="collectionKeyExpression">each collection has to have a key defined</param>
        /// <param name="relationKeyExpression">optional key for each <typeparamref name="TPersistableRelation" /></param>
        /// <typeparam name="TTimeSliceCollection">
        ///     <see cref="TimeDependentParentChildCollection{TRelation,TParent,TChild}" />
        /// </typeparam>
        /// <typeparam name="TPersistableRelation">
        ///     <see cref="TimeDependentRelation{TParent,TChild}" />
        /// </typeparam>
        /// <typeparam name="TPersistableParent">
        ///     <see cref="TimeDependentRelation{TParent,TChild}.Parent" />
        /// </typeparam>
        /// <typeparam name="TPersistableChild">
        ///     <see cref="TimeDependentRelation{TParent,TChild}.Child" />
        /// </typeparam>
        /// <typeparam name="TParentKey"><see cref="IHasKey{TKey}.Id" /> of <typeparamref name="TPersistableParent" /></typeparam>
        /// <typeparam name="TChildKey"><see cref="IHasKey{TKey}.Id" /> of <typeparamref name="TPersistableChild" /></typeparam>
        public static void SetDefaultKeys<TTimeSliceCollection, TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>(
            this ModelBuilder modelBuilder,
            [NotNull] Expression<Func<TTimeSliceCollection, object>> collectionKeyExpression,
            Expression<Func<TPersistableRelation, object>> relationKeyExpression = null)
            where TTimeSliceCollection : PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild,
                TChildKey>
            where TPersistableRelation : class, ITimeSlice, IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>
            where TPersistableParent : class, IHasKey<TParentKey>
            where TPersistableChild : class, IHasKey<TChildKey>
        {
            modelBuilder.Entity<TPersistableParent>().HasKey(parent => parent.Id);
            modelBuilder.Entity<TPersistableChild>().HasKey(child => child.Id);
            modelBuilder.Entity<TPersistableRelation>().HasKey(x => new { x.Discriminator, x.ParentId, x.ChildId, x.Start, x.End });
            modelBuilder.Entity<TPersistableRelation>()
                .HasDefaultKeys<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>(relationKeyExpression);
            modelBuilder.Entity<TTimeSliceCollection>()
                .HasDefaultKeys<TTimeSliceCollection, TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>(collectionKeyExpression);
        }
    }
}