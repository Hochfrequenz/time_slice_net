using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeSlice;

namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     Extensions to specify a key for <see cref="IParentChildRelationship{TParent,TChild}" /> and <see cref="TimeDependentParentChildCollection{TRelationship,TParent,TChild}" />
    /// </summary>
    public static class EntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Adds default keys for a <typeparamref name="TRelationship" /> using <see cref="EntityTypeBuilder.HasKey" />
        /// </summary>
        /// <param name="etb"></param>
        /// <param name="collectionKeyExpression">key of the collection, must not be null</param>
        /// <typeparam name="TRelationship"><see cref="IParentChildRelationship{TParent,TChild}"/></typeparam>
        /// <typeparam name="TParent"><see cref="IParentChildRelationship{TParent,TChild}.Parent"/></typeparam>
        /// <typeparam name="TChild"><see cref="IParentChildRelationship{TParent,TChild}.Child"/></typeparam>
        /// <typeparam name="TTimeSliceCollection"><see cref="TimeDependentParentChildCollection{TRelationship,TParent,TChild}"/></typeparam>
        public static void HasDefaultKeys<TTimeSliceCollection, TRelationship, TParent, TChild>(this EntityTypeBuilder<TTimeSliceCollection> etb,
            [NotNull] Expression<Func<TTimeSliceCollection, object>> collectionKeyExpression)
            where TTimeSliceCollection : TimeDependentParentChildCollection<TRelationship, TParent, TChild>
            where TRelationship : class, ITimeSlice, IParentChildRelationship<TParent, TChild>
            where TParent : class
            where TChild : class
        {
            etb.HasOne(x => x.CommonParent);
            etb.HasMany(x => x.TimeSlices);
            etb.HasKey(collectionKeyExpression ?? throw new ArgumentNullException(nameof(collectionKeyExpression),
                $"Each {nameof(TimeDependentParentChildCollection<TRelationship, TParent, TChild>)}, especially {typeof(TTimeSliceCollection).FullName} has to have a separate key."));
        }

        /// <summary>
        ///     Adds default keys for <typeparamref name="TRelationship" /> using <see cref="EntityTypeBuilder.HasKey" />
        /// </summary>
        /// <param name="etb"></param>
        /// <param name="relationshipKeyExpression">optional key of each <typeparamref name="TRelationship"/>, can be null</param>
        /// <typeparam name="TRelationship"><see cref="IParentChildRelationship{TParent,TChild}" /> </typeparam>
        /// <typeparam name="TParent"><see cref="IParentChildRelationship{TParent,TChild}.Parent" /> </typeparam>
        /// <typeparam name="TChild"><see cref="IParentChildRelationship{TParent,TChild}.Child" /></typeparam>
        public static void HasDefaultKeys<TRelationship, TParent, TChild>(this EntityTypeBuilder<TRelationship> etb, Expression<Func<TRelationship, object>> relationshipKeyExpression = null)
            where TRelationship : class, ITimeSlice, IParentChildRelationship<TParent, TChild>
            where TParent : class
            where TChild : class
        {
            etb.HasDiscriminator(x => x.Discriminator);
            etb.UsePropertyAccessMode(PropertyAccessMode.PreferProperty); // required for the discriminator to be not null
            etb.HasOne(x => x.Parent);
            etb.HasOne(x => x.Child);
            etb.HasKey(relationshipKeyExpression ?? (x => new { x.Start, x.End, x.Discriminator }));
            if (relationshipKeyExpression != null)
            {
                // if the key is specified, then at least set the default key as an index
                etb.HasIndex(x => new { x.Start, x.End, x.Discriminator });
            }
        }
    }
}