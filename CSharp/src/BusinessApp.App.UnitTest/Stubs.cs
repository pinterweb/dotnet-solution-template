namespace BusinessApp.App.UnitTest
{
    using System.ComponentModel.DataAnnotations;

    public class CommandStub
    {}

    [Authorize]
    public class AuthCommandStub : CommandStub
    {}

    [Authorize("Foo", "Bar")]
    public class AuthRolesCommandStub : CommandStub
    {}

    public class DataAnnotatedCommandStub
    {
        [StringLength(10)]
        public string Foo { get; set; }

        [Required]
        public string Bar { get; set; } = "lorem";
    }

    public class QueryStub : IQuery<ResponseStub>
    {}

    public class ResponseStub
    {}
}
