using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
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
        ///     <see cref="TimeDependentCollection{TRelation,TParent,TChild}" />
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
        public static void SetupCollectionAndRelations<TTimeSliceCollection, TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>(
            this ModelBuilder modelBuilder,
            [NotNull] Expression<Func<TTimeSliceCollection, object>> collectionKeyExpression,
            Expression<Func<TPersistableRelation, object>> relationKeyExpression = null)
            where TTimeSliceCollection : PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>
            where TPersistableRelation : class, ITimeSlice, IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>
            where TPersistableParent : class, IHasKey<TParentKey>
            where TPersistableChild : class, IHasKey<TChildKey>
        {
            // set primary keys for parent and child
            modelBuilder.Entity<TPersistableParent>().HasKey(parent => parent.Id);
            modelBuilder.Entity<TPersistableChild>().HasKey(child => child.Id);

            // set primary key for relation
            modelBuilder.Entity<TPersistableRelation>().HasKey(x => new { x.Discriminator, x.ParentId, x.ChildId, x.Start, x.End });
            modelBuilder.Entity<TPersistableRelation>()
                .HasDefaultKeys<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>(relationKeyExpression);
            // autogenerate parent and child id in relation
            modelBuilder.Entity<TPersistableRelation>().Property(x => x.ParentId)
                .HasValueGenerator<ParentIdValueGenerator<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild,
                    TChildKey>>();
            modelBuilder.Entity<TPersistableRelation>().Property(x => x.ChildId)
                .HasValueGenerator<ChildIdValueGenerator<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild,
                    TChildKey>>();

            // set primary key for collection
            modelBuilder.Entity<TTimeSliceCollection>()
                .HasDefaultKeys<TTimeSliceCollection, TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>(collectionKeyExpression);
            // autogenerate common parent id property for collection
            modelBuilder.Entity<TTimeSliceCollection>().Property(x => x.CommonParentId)
                .HasValueGenerator<CommonParentIdValueGenerator<TTimeSliceCollection, TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>>();
        }
    }

    /// <summary>
    ///     The CommonParentIdValueGenerator extracts the
    ///     <see cref="PersistableTimeDependentCollection{TPersistableRelation,TPersistableParent,TParentKey,TPersistableChild,TChildKey}.CommonParentId" /> from the
    ///     <see cref="TimeDependentCollection{TRelation,TParent,TChild}.CommonParent" />
    /// </summary>
    /// <typeparam name="TTimeSliceCollection"></typeparam>
    /// <typeparam name="TPersistableRelation"></typeparam>
    /// <typeparam name="TPersistableParent"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    /// <typeparam name="TPersistableChild"></typeparam>
    /// <typeparam name="TChildKey"></typeparam>
    internal class CommonParentIdValueGenerator<TTimeSliceCollection, TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild,
        TChildKey> : ValueGenerator<TParentKey>
        where TTimeSliceCollection : PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>
        where TPersistableRelation : class, ITimeSlice, IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>
        where TPersistableParent : class, IHasKey<TParentKey>
        where TPersistableChild : class, IHasKey<TChildKey>
    {
        public override bool GeneratesTemporaryValues => false;

        public override TParentKey Next(EntityEntry entry)
        {
            if (entry.Entity is TTimeSliceCollection collection) return collection.CommonParent.Id;

            throw new NotImplementedException($"Generating a default value for {entry?.Entity?.GetType()} is not implemented.");
        }
    }

    /// <summary>
    ///     The ParentIdValueGenerator extracts the <see cref="PersistableTimeDependentRelation{TPersistableParent,TParentKey,TPersistableChild,TChildKey}.ParentId" /> from the
    ///     <see cref="TimeDependentRelation{TParent,TChild}.Parent" />
    /// </summary>
    /// <typeparam name="TPersistableRelation"></typeparam>
    /// <typeparam name="TPersistableParent"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    /// <typeparam name="TPersistableChild"></typeparam>
    /// <typeparam name="TChildKey"></typeparam>
    internal class ParentIdValueGenerator<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild,
        TChildKey> : ValueGenerator<TParentKey>
        where TPersistableRelation : class, ITimeSlice, IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>
        where TPersistableParent : class, IHasKey<TParentKey>
        where TPersistableChild : class, IHasKey<TChildKey>
    {
        public override bool GeneratesTemporaryValues => false;

        public override TParentKey Next(EntityEntry entry)
        {
            if (entry.Entity is TPersistableRelation relation) return relation.Parent.Id;

            throw new NotImplementedException($"Generating a default value for {entry?.Entity?.GetType()} is not implemented.");
        }
    }

    /// <summary>
    ///     The ParentIdValueGenerator extracts the <see cref="PersistableTimeDependentRelation{TPersistableParent,TParentKey,TPersistableChild,TChildKey}.ChildId" /> from the
    ///     <see cref="TimeDependentRelation{TParent,TChild}.Child" />
    /// </summary>
    /// <typeparam name="TPersistableRelation"></typeparam>
    /// <typeparam name="TPersistableParent"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    /// <typeparam name="TPersistableChild"></typeparam>
    /// <typeparam name="TChildKey"></typeparam>
    internal class ChildIdValueGenerator<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild,
        TChildKey> : ValueGenerator<TChildKey>
        where TPersistableRelation : class, ITimeSlice, IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>
        where TPersistableParent : class, IHasKey<TParentKey>
        where TPersistableChild : class, IHasKey<TChildKey>
    {
        public override bool GeneratesTemporaryValues => false;

        public override TChildKey Next(EntityEntry entry)
        {
            if (entry.Entity is TPersistableRelation relation) return relation.Child.Id;

            throw new NotImplementedException($"Generating a default value for {entry?.Entity?.GetType()} is not implemented.");
        }
    }
}