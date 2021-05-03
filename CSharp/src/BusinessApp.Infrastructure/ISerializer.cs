namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Service providing Deserializing and Serializing logic
    /// </summary>
    public interface ISerializer
    {
        T? Deserialize<T>(byte[] data);
        byte[] Serialize<T>(T graph);
    }
}
