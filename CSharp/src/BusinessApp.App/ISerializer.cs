namespace BusinessApp.App
{
    public interface ISerializer
    {
        T Deserialize<T>(byte[] data);
        byte[] Serialize<T>(T graph);
    }
}
