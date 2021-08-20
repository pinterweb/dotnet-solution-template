using Xunit;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace BusinessApp.Infrastructure.IntegrationTest.Json
{
    using BusinessApp.Infrastructure.Json;

    public class NewtonsoftJsonSerializerTests
    {
        private readonly JsonSerializerSettings settings;
        private readonly NewtonsoftJsonSerializer sut;

        public NewtonsoftJsonSerializerTests()
        {
            settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            sut = new NewtonsoftJsonSerializer(settings);
        }

        public class Deserialize : NewtonsoftJsonSerializerTests
        {
            [Fact]
            public void ModelDeserializedWithSettings()
            {
                /* Arrange */
                using var ms = new MemoryStream();
                using var sw = new StreamWriter(ms);
                sw.Write("{\"Foo\":1}");
                sw.Flush();
                ms.Position = 0;

                /* Act */
                var result = sut.Deserialize<TestModel>(ms.GetBuffer());

                /* Assert */
                Assert.Equal(1, result.Foo);
            }

            [Fact]
            public void OnError_ThrowsJsonDeserializationException()
            {
                /* Arrange */
                using var ms = new MemoryStream();
                using var sw = new StreamWriter(ms);
                sw.Write("{\"foo\":\"foo\"}");
                sw.Flush();
                ms.Position = 0;

                /* Act */
                var ex = Record.Exception(() => sut.Deserialize<TestModel>(ms.GetBuffer()));

                /* Assert */
                var error = Assert.IsType<JsonDeserializationException>(ex);
            }

            [Fact]
            public void OnError_HasOriginalException()
            {
                /* Arrange */
                using var ms = new MemoryStream();
                using var sw = new StreamWriter(ms);
                sw.Write("{\"foo\":\"foo\"}");
                sw.Flush();
                ms.Position = 0;

                /* Act */
                var ex = Record.Exception(() => sut.Deserialize<TestModel>(ms.GetBuffer()));

                /* Assert */
                var error = Assert.IsType<JsonReaderException>(ex.InnerException);
            }
        }

        public class Serialize : NewtonsoftJsonSerializerTests
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
                _ = Assert.IsType<Newtonsoft.Json.JsonSerializationException>(error.InnerException);
            }
        }

        public class TestModel
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }
    }
}
