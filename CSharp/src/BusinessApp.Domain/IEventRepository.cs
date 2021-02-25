namespace BusinessApp.Domain
{
    /// <summary>
    /// Repository pattern for <see cref="IDomainEvent"/> objects
    /// </summary>
    public interface IEventRepository
    {
        void Add<T>(T @event) where T : IDomainEvent;
    }
}
