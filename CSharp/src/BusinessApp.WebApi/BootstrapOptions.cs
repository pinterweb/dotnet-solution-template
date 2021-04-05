namespace BusinessApp.WebApi
{
    using System.Collections.Generic;
    using System.Reflection;
    using BusinessApp.Domain;

    public sealed class BootstrapOptions
    {
        public BootstrapOptions(string connStr)
        {
            DbConnectionString = connStr
                .NotEmpty()
                .Expect("A data connection string is bootstrap to start this application");
        }

        public string DbConnectionString { get; set; }
        public string? LogFilePath { get; set; }
        public IEnumerable<Assembly> RegistrationAssemblies { get; init; } = new List<Assembly>();
    }
}
