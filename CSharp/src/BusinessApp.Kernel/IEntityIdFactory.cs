namespace BusinessApp.Kernel
{
    /// <summary>
    /// Factory to generate unique ids
    /// </summary>
    public interface IEntityIdFactory<T> where T : IEntityId
    {
        T Create();
    }
}
