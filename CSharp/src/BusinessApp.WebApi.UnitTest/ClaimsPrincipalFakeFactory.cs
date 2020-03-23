namespace BusinessApp.WebApi.UnitTest
{
  using System.Security.Claims;
  using FakeItEasy;

  public static class ClaimsPrincipalFakeFactory
  {
    public static ClaimsPrincipal New(ClaimsIdentity identity = null)
    {
      var fakeCp = A.Fake<ClaimsPrincipal>();

      if (identity == null)
      {
        identity = ClaimsIdentityFakeFactory.New();
      }

      A.CallTo(() => fakeCp.Identity).Returns(identity);

      return fakeCp;
    }
  }
}
