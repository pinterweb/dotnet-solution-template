namespace BusinessApp.App.UnitTest
{
    using Xunit;
    using FakeItEasy;
    using BusinessApp.App;

    public class MemberValidationExceptionExtensionsTests
    {
        public class CreateWithIndexName : MemberValidationExceptionExtensionsTests
        {
            [Fact]
            public void IndexArg_AddedToMemberName()
            {
                /* Arrange */
                var ex = new MemberValidationException("foo", A.CollectionOfDummy<string>(1));

                /* Act */
                var indexedEx = ex.CreateWithIndexName(22);

                /* Assert */
                Assert.Equal("[22].foo", indexedEx.MemberName);
            }
        }
    }
}
