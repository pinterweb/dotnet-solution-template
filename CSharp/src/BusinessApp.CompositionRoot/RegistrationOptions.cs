using System.Collections.Generic;
using System.Reflection;
using BusinessApp.Kernel;

namespace BusinessApp.CompositionRoot
{
    public sealed class RegistrationOptions
    {
        public RegistrationOptions(string connStr, string envName)
        {
            DbConnectionString = connStr
                .NotEmpty()
                .Expect("A data connection string is bootstrap to start this application");

            EnvironmentName = envName ?? "Development";
        }

        public string DbConnectionString { get; set; }
        public string EnvironmentName { get; set; }
        public IEnumerable<Assembly> RegistrationAssemblies { get; init; } = new List<Assembly>();
    }
}
