using FakeItEasy;
using BusinessApp.Infrastructure;

namespace BusinessApp.Test.Shared
{
    public class ModelValidationExceptionDummyFactory : DummyFactory<MemberValidationException>
    {
        protected override MemberValidationException Create()
        {
            return new MemberValidationException("foo", A.CollectionOfDummy<string>(1));
        }
    }
}
