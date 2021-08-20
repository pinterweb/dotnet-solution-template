using Xunit;
using System.IO;
using System.Text;
using System.Text.Json;

namespace BusinessApp.Infrastructure.IntegrationTest.Json
{
    using BusinessApp.Infrastructure.Json;

    public class SystemJsonSerializerAdapterTests
    {
        private readonly JsonSerializerOptions settings;
        private readonly SystemJsonSerializerAdapter sut;

        public SystemJsonSerializerAdapterTests()
        {
            settings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            sut = new SystemJsonSerializerAdapter(settings);
        }

        public class Deserialize : SystemJsonSerializerAdapterTests
        {
            [Fact]
            public void ModelDeserializedWithSettings()
            {
                /* Arrange */
                var json = "{\"foo\":1}";
                var data = Encoding.UTF8.GetBytes(json);

                /* Act */
                var result = sut.Deserialize<TestModel>(data);

                /* Assert */
                Assert.Equal(1, result.Foo);
            }

            [Fact]
            public void OnError_ThrowsJsonDeserializationException()
            {
                /* Arrange */
                var json = "{\"foo\":\"foo\"}";
                var data = Encoding.UTF8.GetBytes(json);

                /* Act */
                var ex = Record.Exception(() => sut.Deserialize<TestModel>(data));

                /* Assert */
                var error = Assert.IsType<JsonDeserializationException>(ex);
            }

            [Fact]
            public void OnError_HasOriginalException()
            {
                /* Arrange */
                var json = "{\"foo\":\"foo\"}";
                var data = Encoding.UTF8.GetBytes(json);

                /* Act */
                var ex = Record.Exception(() => sut.Deserialize<TestModel>(data));

                /* Assert */
                var error = Assert.IsType<JsonException>(ex.InnerException);
            }
        }

        public class Serialize : SystemJsonSerializerAdapterTests
        {
            [Fact]
            public void WithTestModel_ConvertsToJsonString()
            {
                /* Arrange */
                var model = new TestModel { Foo = 1, Bar = 2 };

                /* Act */
                var buffer = sut.Serialize(model);

                /* Assert */
                using var ms = new MemoryStream(buffer);
                using var sr = new StreamReader(ms);
                var payload = sr.ReadToEnd();
                Assert.Equal("{\"foo\":1,\"bar\":2}", payload);
            }

            [Fact]
            public void OnError_ThrowsJsonSerializationException()
            {
                /* Arrange */
                var model = new DirectoryInfo("./");

                /* Act */
                var error = Record.Exception(() => sut.Serialize(model));

                /* Assert */
                _ = Assert.IsType<JsonSerializationException>(error);
            }

            [Fact]
            public void OnError_HasOriginalException()
            {
                /* Arrange */
                var model = new DirectoryInfo("./");

                /* Act */
                var error = Record.Exception(() => sut.Serialize(model));

                /* Assert */
                _ = Assert.IsType<JsonException>(error.InnerException);
            }
        }

        public class TestModel
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }
    }
}
