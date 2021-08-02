using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     Extensions to specify a key for <see cref="IParentChildRelationship{TParent,TChild}" /> and <see cref="TimeDependentParentChildCollection{TRelationship,TParent,TChild}" />
    /// </summary>
    public static class HasDefaultKeyExtensions
    {
        /// <summary>
        ///     Adds default keys for a <typeparamref name="TRelationship" /> using <see cref="EntityTypeBuilder.HasKey" />
        /// </summary>
        /// <param name="etb"></param>
        /// <param name="keyExpression">optional key if defined</param>
        /// <typeparam name="TRelationship"></typeparam>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TChild"></typeparam>
        /// <typeparam name="TTimeSliceCollection"></typeparam>
        public static void HasDefaultKeys<TTimeSliceCollection, TRelationship, TParent, TChild>(this EntityTypeBuilder<TTimeSliceCollection> etb,
            Expression<Func<TTimeSliceCollection, object>> keyExpression = null)
            where TTimeSliceCollection : TimeDependentParentChildCollection<TRelationship, TParent, TChild>
            where TRelationship : class, ITimeSlice, IParentChildRelationship<TParent, TChild>
            where TParent : class
            where TChild : class
        {
            etb.HasOne(x => x.CommonParent);
            etb.HasMany(x => x.TimeSlices);
            etb.HasKey(keyExpression ?? throw new ArgumentNullException(nameof(keyExpression),
                $"Any {nameof(TimeDependentParentChildCollection<TRelationship, TParent, TChild>)} has to have a separate key."));
        }

        /// <summary>
        ///     Adds default keys for a
        ///     <typeparamref name="TRelationship" />
        ///     using
        ///     <see cref="EntityTypeBuilder.HasKey" />
        /// </summary>
        /// <param name="etb"></param>
        /// <param name="keyExpression">optional key if defined</param>
        /// <typeparam name="TRelationship">
        ///     <see cref="IParentChildRelationship{TParent,TChild}" />
        /// </typeparam>
        /// <typeparam name="TParent">
        ///     <see cref="IParentChildRelationship{TParent,TChild}" />
        /// </typeparam>
        /// <typeparam name="TChild">
        ///     <see cref="IParentChildRelationship{TParent,TChild}" />
        /// </typeparam>
        public static void HasDefaultKeys<TRelationship, TParent, TChild>(this EntityTypeBuilder<TRelationship> etb, Expression<Func<TRelationship, object>> keyExpression = null)
            where TRelationship : class, ITimeSlice, IParentChildRelationship<TParent, TChild>
            where TParent : class
            where TChild : class
        {
            etb.HasDiscriminator(x => x.Discriminator);
            etb.UsePropertyAccessMode(PropertyAccessMode.PreferProperty); // required for the discriminator to be not null
            etb.HasOne(x => x.Parent);
            etb.HasOne(x => x.Child);
            etb.HasKey(keyExpression ?? (x => new { x.Start, x.End, x.Discriminator }));
        }
    }
}