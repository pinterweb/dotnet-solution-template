namespace BusinessApp.WebApi
{
    using System.Reflection;

    public sealed class BootstrapOptions
    {
        public string DbConnectionString { get; set; }
        public string LogFilePath { get; set; }
        public Assembly AppLayerAssembly { get; set; }
    }
}
