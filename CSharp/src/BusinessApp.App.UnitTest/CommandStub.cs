namespace BusinessApp.App.UnitTest
{
    public class CommandStub
    {}

    [Authorize]
    public class AuthCommandStub : CommandStub
    {}

    [Authorize("Foo", "Bar")]
    public class AuthRolesCommandStub : CommandStub
    {}
}
