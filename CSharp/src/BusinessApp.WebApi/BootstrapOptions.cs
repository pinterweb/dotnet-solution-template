namespace BusinessApp.WebApi
{
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class BootstrapOptions
    {
        public string DbConnectionString { get; set; }
        public string LogFilePath { get; set; }
        public IEnumerable<Assembly> AppAssemblies { get; set; }
        public IEnumerable<Assembly> DataAssemblies { get; set; }
    }
}
