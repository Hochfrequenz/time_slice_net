using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     Extensions to specify a key for <see cref="IRelation{TParent,TChild}" /> and <see cref="TimeDependentParentChildCollection{TRelation,TParent,TChild}" />
    /// </summary>
    public static class EntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Adds default keys for a <typeparamref name="TPersistableRelation" /> using <see cref="EntityTypeBuilder.HasKey" />
        /// </summary>
        /// <param name="etb"></param>
        /// <param name="collectionKeyExpression">key of the collection, must not be null</param>
        /// <typeparam name="TPersistableRelation">
        ///     <see cref="IRelation{TParent,TChild}" />
        /// </typeparam>
        /// <typeparam name="TPersistableParent">
        ///     <see cref="IRelation{TParent,TChild}.Parent" />
        /// </typeparam>
        /// <typeparam name="TPersistableChild">
        ///     <see cref="IRelation{TParent,TChild}.Child" />
        /// </typeparam>
        /// <typeparam name="TTimeSliceCollection">
        ///     <see cref="TimeDependentParentChildCollection{TRelation,TParent,TChild}" />
        /// </typeparam>
        /// <typeparam name="TParentKey"></typeparam>
        /// <typeparam name="TChildKey"></typeparam>
        public static void HasDefaultKeys<TTimeSliceCollection, TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>(
            this EntityTypeBuilder<TTimeSliceCollection> etb,
            [NotNull] Expression<Func<TTimeSliceCollection, object>> collectionKeyExpression)
            where TTimeSliceCollection : PersistableTimeDependentCollection<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>
            where TPersistableRelation : class, ITimeSlice, IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>
            where TPersistableParent : class, IHasKey<TParentKey>
            where TPersistableChild : class, IHasKey<TChildKey>
        {
            etb.HasOne(x => x.CommonParent);
            etb.HasMany(x => x.TimeSlices);
            etb.HasKey(collectionKeyExpression ?? throw new ArgumentNullException(nameof(collectionKeyExpression),
                $"Each {nameof(TimeDependentParentChildCollection<TPersistableRelation, TPersistableParent, TPersistableChild>)}, especially {typeof(TTimeSliceCollection).FullName} has to have a separate key."));
        }

        /// <summary>
        ///     Adds default keys for <typeparamref name="TPersistableRelation" /> using <see cref="EntityTypeBuilder.HasKey" />
        /// </summary>
        /// <param name="etb"></param>
        /// <param name="relationKeyExpression">optional key of each <typeparamref name="TPersistableRelation" />, can be null</param>
        /// <typeparam name="TPersistableRelation">
        ///     <see cref="IRelation{TParent,TChild}" />
        /// </typeparam>
        /// <typeparam name="TPersistableParent">
        ///     <see cref="IRelation{TParent,TChild}.Parent" />
        /// </typeparam>
        /// <typeparam name="TPersistableChild">
        ///     <see cref="IRelation{TParent,TChild}.Child" />
        /// </typeparam>
        /// <typeparam name="TParentKey"></typeparam>
        /// <typeparam name="TChildKey"></typeparam>
        public static void HasDefaultKeys<TPersistableRelation, TPersistableParent, TParentKey, TPersistableChild, TChildKey>(
            this EntityTypeBuilder<TPersistableRelation> etb,
            Expression<Func<TPersistableRelation, object>> relationKeyExpression = null)
            where TPersistableRelation : class, ITimeSlice, IPersistableRelation<TPersistableParent, TParentKey, TPersistableChild, TChildKey>
            where TPersistableParent : class, IHasKey<TParentKey>
            where TPersistableChild : class, IHasKey<TChildKey>
        {
            etb.HasDiscriminator(x => x.Discriminator);
            etb.UsePropertyAccessMode(PropertyAccessMode.PreferProperty); // required for the discriminator to be not null
            etb.HasOne(x => x.Parent);
            etb.HasOne(x => x.Child);
            etb.HasKey(relationKeyExpression ?? (x => new { x.Discriminator, x.ParentId, x.ChildId, x.Start, x.End }));
            if (relationKeyExpression != null)
                // if the key is explicitly specified, then at least set the default key as an index
                etb.HasIndex(x => new { x.Discriminator, x.ParentId, x.ChildId, x.Start, x.End });
        }
    }
}