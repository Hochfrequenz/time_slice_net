using System;
using TimeSlice.Converters;

namespace TimeSlice
{
    /// <summary>
    /// The simplest implementation of a time dependent parent child relationship.
    /// </summary>
    public class TimeDependentParentChildRelationship<TParent, TChild> : ITimeSlice,
        IParentChildRelationship<TParent, TChild> where TParent : class where TChild : class
    {
        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("start")]
        [System.Text.Json.Serialization.JsonConverter(typeof(EnforceTimeZoneOffsetConverter))]
        public DateTimeOffset Start { get; set;  }


        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("end")]
        [System.Text.Json.Serialization.JsonConverter(typeof(NullableEnforceTimeZoneOffsetConverter))]
        public DateTimeOffset? End { get; set; }


        /// <inheritdoc />
        [System.Text.Json.Serialization.JsonPropertyName("discriminator")]
        public string Discriminator { get; set;  }

        /// <summary>
        /// The entity that "owns" / has assigned <see cref="Child"/> in between [<see cref="Start"/> and <see cref="End"/>)
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("parent")]
        public TParent Parent { get; set;  }

        /// <summary>
        /// The entity that is owned by / assigned to <see cref="Parent"/> in between [<see cref="Start"/> and <see cref="End"/>)
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("child")]
        public TChild Child { get; set;  }
    }
}