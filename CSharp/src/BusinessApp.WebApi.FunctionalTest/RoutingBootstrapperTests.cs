namespace BusinessApp.WebApi.FunctionalTest
{
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;

    public class RoutingBootstrapperTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;
        private readonly ITestOutputHelper output;

        public RoutingBootstrapperTests(WebApplicationFactory<Startup> factory,
            ITestOutputHelper output)
        {
            this.factory = factory;
            this.output = output;
        }

        [Fact]
        public async Task GivenARequestToGetOneResource_WhenIsValidQuery_ReturnsOneResult()
        {
            // Given
            var client = factory.NewClient();
            var payload = new { id = 1 };

            // When
            var response = await client.GetAsync("/api/resources/1");

            // Then
            var resource = await response.Success<Response>(output);
            Assert.Equal(1, resource.Id);
        }

        [Fact]
        public async Task GivenARequestToGetManyResources_WhenIsAnEnvelope_HeadersReturnedToo()
        {
            // Given
            var client = factory.NewClient();
            var payload = new { id = 1 };

            // When
            var response = await client.GetAsync("/api/resources");

            // Then
            Assert.True(response.IsSuccessStatusCode);
            var accessHeader = Assert.Single(
                response.Headers.GetValues("Access-Control-Expose-Headers"));
            var pageHeader = Assert.Single(
                response.Headers.GetValues("VND.parkeremg.pagination"));
            Assert.Equal("VND.parkeremg.pagination", accessHeader);
            Assert.Equal("count=1", pageHeader);
        }

        [Fact]
        public async Task GivenARequestToSaveANewResource_WhenIsValidRequest_EchosResut()
        {
            // Given
            var client = factory.NewClient();
            var payload = new { id = 1, longerId = 99 };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await client.PostAsync("/api/resources", content);

            // Then
            Assert.True(response.IsSuccessStatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\r\n  \"longerId\": \"99\",\r\n  \"id\": 1\r\n}", json);
        }

        [Fact]
        public async Task GivenARequestToReplaceAResource_WhenIsSuccessful_NoContentReturned()
        {
            // Given
            var client = factory.NewClient();
            var payload = new { id = 1 };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await client.PutAsync("/api/resources/1", content);

            // Then
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GivenARequestToDeleteAResource_WhenIsSuccessful_NoContentReturned()
        {
            // Given
            var client = factory.NewClient();
            var payload = new { id = 1 };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await client.DeleteAsync("/api/resources/1");

            // Then
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public class Response
        {
            public int Id { get; set; }
        }
    }
}
