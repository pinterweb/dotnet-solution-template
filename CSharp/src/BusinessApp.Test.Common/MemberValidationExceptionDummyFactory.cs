namespace BusinessApp.Test
{
    using FakeItEasy;
    using BusinessApp.App;

    public class ModelValidationExceptionDummyFactory : DummyFactory<MemberValidationException>
    {
        protected override MemberValidationException Create()
        {
            return new MemberValidationException("foo", A.CollectionOfDummy<string>(1));
        }
    }
}
