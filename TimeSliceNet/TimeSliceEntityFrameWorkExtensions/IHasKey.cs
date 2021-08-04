namespace TimeSliceEntityFrameworkExtensions
{
    /// <summary>
    ///     Interface ensures that a class that implements it, has a Property <see cref="Id" /> of type <typeparamref name="TKey" /> that can be used as a database primary/foreign/part of
    ///     a composite key in Entity Framework
    /// </summary>
    /// <typeparam name="TKey">key type, e.g. string or Guid</typeparam>
    public interface IHasKey<TKey>
    {
        /// <summary>
        ///     Unique ID of this entry on a database
        /// </summary>
        public TKey Id { get; protected set; }
    }
}