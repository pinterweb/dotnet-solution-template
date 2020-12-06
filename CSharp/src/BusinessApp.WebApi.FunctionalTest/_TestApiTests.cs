namespace BusinessApp.WebApi.FunctionalTest
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;

    // TODO set functional tests up
    public class TestApiTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;
        private readonly ITestOutputHelper output;

        public TestApiTests(
            WebApplicationFactory<Startup> factory,
            ITestOutputHelper output)
        {
            this.factory = factory;
            this.output = output;
        }

        [Fact]
        public async Task GivenANewTicket_WhenItIsValid_ItGetsSavedToTheDatabase()
        {
            // Given
            var client = factory.NewClient();
            var payload = new
            {
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await client.PostAsync("/api/_tests", content);

            // Then
            // var resource = await response.Success<Reesource>(output);
            Assert.True(response.IsSuccessStatusCode);
        }
        [Fact]
        public async Task GivenARequest_WhenItIsValid_ReturnsPayload()
        {
            // Given
            var client = factory.NewClient();

            // When
            var response = await client.GetAsync("/api/_test");

            // Then
            output.WriteLine("=============================");
            output.WriteLine($"StatusCode: {response.StatusCode}");
            output.WriteLine("payload: ");
            output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}
