namespace BusinessApp.Domain
{
    /// <summary>
    /// Repository pattern for <see cref="IDomainEvent"/> objects
    /// </summary>
    public interface IEventRepository
    {
        void Add(IDomainEvent @event);
    }
}
