using Microsoft.AspNetCore.Http;
using FakeItEasy;
using FakeItEasy.Creation;
using System.Security.Claims;

namespace BusinessApp.WebApi.UnitTest
{
    public class HttpContextFakeFactory : FakeOptionsBuilder<HttpContext>
    {
        protected override void BuildOptions(IFakeOptions<HttpContext> options)
        {
            var fakeReq = A.Fake<HttpRequest>();
            var fakeRes = A.Fake<HttpResponse>();
            var principal = A.Fake<ClaimsPrincipal>();

            A.CallTo(() => fakeReq.Scheme).Returns("https");
            A.CallTo(() => fakeReq.Host).Returns(new HostString("foobar"));

            options.ConfigureFake(fakeCtx =>
            {
                A.CallTo(() => fakeCtx.Request).Returns(fakeReq);
                A.CallTo(() => fakeCtx.Response).Returns(fakeRes);
                A.CallTo(() => fakeCtx.User).Returns(principal);
            });
        }
    }
}
