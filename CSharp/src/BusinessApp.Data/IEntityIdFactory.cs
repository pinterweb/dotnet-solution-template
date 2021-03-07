namespace BusinessApp.Data
{
    using BusinessApp.Domain;

    /// <summary>
    /// Factory to generate unique ids
    /// </summary>
    public interface IEntityIdFactory<T> where T : IEntityId
    {
        T Create();
    }
}
