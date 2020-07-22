namespace BusinessApp.App.UnitTest
{
    public class CommandStub
    {}

    [Obsolete("Use CommandStub instead")]
    public class DummyCommand { }

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
