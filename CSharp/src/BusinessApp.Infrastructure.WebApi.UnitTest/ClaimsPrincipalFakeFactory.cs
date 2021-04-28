using System.Security.Claims;
using FakeItEasy;
using FakeItEasy.Creation;

namespace BusinessApp.Infrastructure.WebApi.UnitTest
{
    public class ClaimsPrincipalFakeFactory : FakeOptionsBuilder<ClaimsPrincipal>
    {
        protected override void BuildOptions(IFakeOptions<ClaimsPrincipal> options)
        {
            options.ConfigureFake(fake =>
            {
                A.CallTo(() => fake.Identity).Returns(A.Fake<ClaimsIdentity>());
            });
        }
    }
}
