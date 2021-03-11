namespace BusinessApp.WebApi.FunctionalTest
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;

    public static class HttpResponseMessageExtensions
    {
        public static async Task Success(this HttpResponseMessage response,
            ITestOutputHelper output)
        {
            var json = await response.Content.ReadAsStringAsync();

            output.WriteLine("=============================");
            output.WriteLine($"StatusCode: {response.StatusCode}");
            output.WriteLine("payload:");
            output.WriteLine(json);
            Assert.True(response.IsSuccessStatusCode);
        }

        public static async Task<T> Success<T>(this HttpResponseMessage response,
            ITestOutputHelper output)
        {
            var json = await response.Content.ReadAsStringAsync();

            output.WriteLine("=============================");
            output.WriteLine($"StatusCode: {response.StatusCode}");
            output.WriteLine("payload:");
            output.WriteLine(json);
            Assert.True(response.IsSuccessStatusCode);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}

