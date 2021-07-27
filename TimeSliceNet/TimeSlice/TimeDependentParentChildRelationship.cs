using System.Text.Json.Serialization;

namespace TimeSlice
{
    /// <summary>
    ///     The simplest implementation of a time dependent parent child relationship.
    /// </summary>
    public class TimeDependentParentChildRelationship<TParent, TChild> : PlainTimeSlice,
        IParentChildRelationship<TParent, TChild> where TParent : class where TChild : class
    {
        private string _discriminator;

        /// <inheritdoc />
        [JsonPropertyName("discriminator")]
        public string Discriminator
        {
            get => _discriminator ?? $"{typeof(TimeDependentParentChildRelationship<TParent, TChild>).FullName}";
            set
            {
                if (!string.IsNullOrWhiteSpace(value)) _discriminator = value;
            }
        }

        /// <summary>
        ///     The entity that "owns" / has assigned <see cref="Child" /> in between [<see cref="ITimeSlice.Start" /> and <see cref="ITimeSlice.End" />)
        /// </summary>
        [JsonPropertyName("parent")]
        public TParent Parent { get; set; }

        /// <summary>
        ///     The entity that is owned by / assigned to <see cref="Parent" /> in between [<see cref="ITimeSlice.Start" /> and <see cref="ITimeSlice.End" />)
        /// </summary>
        [JsonPropertyName("child")]
        public TChild Child { get; set; }
    }
}