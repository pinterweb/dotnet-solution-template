using BusinessApp.Kernel;

namespace BusinessApp.Analyzers.IntegrationTest
{
    public partial class EntityStub
    {
#nullable enable
        [Id]
        public string? Id { get; set; }
#nullable restore

    }

    public partial struct StructStub
    {
#nullable enable
        [Id]
        public int? Id { get; set; }
#nullable restore

    }
}
