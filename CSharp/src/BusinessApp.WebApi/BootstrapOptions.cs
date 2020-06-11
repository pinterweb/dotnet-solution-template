namespace BusinessApp.WebApi
{
    public sealed class BootstrapOptions
    {
        public string WriteConnectionString { get; set; }
        public string ReadConnectionString { get; set; }
        public string LogFilePath { get; set; }
    }
}
