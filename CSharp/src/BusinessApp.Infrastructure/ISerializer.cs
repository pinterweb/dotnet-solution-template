namespace BusinessApp.Infrastructure
{
    public interface ISerializer
    {
        T? Deserialize<T>(byte[] data);
        byte[] Serialize<T>(T graph);
    }
}
