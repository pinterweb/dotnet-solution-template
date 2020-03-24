namespace BusinessApp.WebApi.UnitTest
{
  using System.Security.Claims;
  using FakeItEasy;

  public static class ClaimsIdentityFakeFactory
  {
    public static ClaimsIdentity New(string userName = "anyone")
    {
      var fakeId = A.Fake<ClaimsIdentity>();
      A.CallTo(() => fakeId.Name).Returns(userName);
      return fakeId;
    }
  }
}
