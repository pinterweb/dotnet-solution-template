using FakeItEasy;
using Xunit;
using BusinessApp.Infrastructure.Json;
using Newtonsoft.Json;
using BusinessApp.Kernel;
using System.IO;
using Newtonsoft.Json.Serialization;

namespace BusinessApp.Infrastructure.IntegrationTest.Json
{
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
            public void NoError_ModelDeserializedWithSettings()
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
            public void OnError_WhenHasMemberName_ThrowsModelValidationException()
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
                var error = Assert.IsType<ModelValidationException>(ex);
            }

            [Fact]
            public void OnError_HasMultipleMembers_ReturnsEachInvalidMember()
            {
                /* Arrange */
                using var ms = new MemoryStream();
                using var sw = new StreamWriter(ms);
                sw.Write("{\"foo\":\"foo\",\"bar\":\"lorem\"}");
                sw.Flush();
                ms.Position = 0;

                /* Act */
                var ex = Record.Exception(() => sut.Deserialize<TestModel>(ms.GetBuffer()));

                /* Assert */
                var error = Assert.IsType<ModelValidationException>(ex);
                Assert.Collection(error,
                    e => Assert.Equal("foo", e.MemberName),
                    e => Assert.Equal("bar", e.MemberName)
                );
            }

            [Fact]
            public void OnError_HasNewtonsoftErrorMessages()
            {
                /* Arrange */
                using var ms = new MemoryStream();
                using var sw = new StreamWriter(ms);
                sw.Write("{\"foo\":\"foo\"}");
                sw.Flush();
                ms.Position = 0;

                /* Act */
                var ex = Record.Exception(() => sut.Deserialize<TestModel>(ms.GetBuffer()));
                var error = Assert.IsType<ModelValidationException>(ex);

                /* Assert TODO can we remove the "Path portion? "*/
                Assert.Collection(error,
                    e => Assert.Contains(
                        "Could not convert string to integer: foo. " +
                        "Path 'foo', line 1, position 12.",
                        e.Errors)
                );
            }

            [Fact]
            public void OnError_WithoutAMemberName_ThrowsOriginalException()
            {
                /* Arrange */
                using var ms = new MemoryStream();
                using var sw = new StreamWriter(ms);
                sw.Write("[{\"foo\":\"foo\"}]");
                sw.Flush();
                ms.Position = 0;

                /* Act */
                var ex = Record.Exception(() => sut.Deserialize<TestModel>(ms.GetBuffer()));

                /* Assert */
                var error = Assert.IsType<JsonSerializationException>(ex);
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
                using var ms = new MemoryStream(buffer);
                using var sr = new StreamReader(ms);
                var payload = sr.ReadToEnd();

                /* Assert */
                Assert.Equal("{\"foo\":1,\"bar\":2}", payload);
            }
        }

        public class TestModel
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }
    }
}
