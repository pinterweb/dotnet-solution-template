namespace BusinessApp.WebApi.UnitTest
{
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;
    using FakeItEasy;

    public static class HttpContextFakeFactory
    {
        public static HttpContext New(ClaimsPrincipal principal = null,
          HttpRequest request = null, HttpResponse response = null)
        {
            var fakeCtx = A.Fake<HttpContext>();
            var fakeReq = A.Fake<HttpRequest>();
            var fakeRes = A.Fake<HttpResponse>();

            A.CallTo(() => fakeCtx.Request).Returns(request ?? fakeReq);
            A.CallTo(() => fakeCtx.Response).Returns(response ?? fakeRes);

            A.CallTo(() => fakeReq.Scheme).Returns("https");
            A.CallTo(() => fakeReq.Host).Returns(new HostString("foobar"));

            if (principal == null)
            {
                principal = ClaimsPrincipalFakeFactory.New();
            }

            A.CallTo(() => fakeCtx.User).Returns(principal);

            return fakeCtx;
        }
    }
}
