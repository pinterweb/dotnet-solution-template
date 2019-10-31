namespace BusinessApp.Domain
{
    /// <summary>
    /// Creates an <see cref="IEntityId" />
    /// </summary>
    public interface IEntityIdFactory<TId>
        where TId: IEntityId
    {
        TId Create();
    }
}
