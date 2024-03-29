﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TimeSlice
{
    /// <summary>
    ///     Extension Methods for <see cref="IValidatableObject" />
    /// </summary>
    public static class ValidatableObjectExtensions
    {
        /// <summary>
        /// This is just a dummy validation context that is not null.
        /// Replace it with a real validation context.
        /// </summary>
        private static ValidationContext _nonNullDummyValidationContext = new(new object());

        /// <summary>
        ///     if the <see cref="IValidatableObject.Validate" /> method accepts a null validation context, then this method allows an easily readable access
        /// </summary>
        /// <param name="validatableObject"></param>
        /// <returns></returns>
        public static bool IsValid(this IValidatableObject validatableObject)
        {
            // valid = there are no entries returned.
            return !validatableObject.Validate(_nonNullDummyValidationContext).Any();
        }
    }
}