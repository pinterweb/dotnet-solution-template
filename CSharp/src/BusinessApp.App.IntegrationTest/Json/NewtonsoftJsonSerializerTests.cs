namespace BusinessApp.App.IntegrationTest.Json
{
    using FakeItEasy;
    using Xunit;
    using BusinessApp.App.Json;
    using Newtonsoft.Json;
    using BusinessApp.Domain;
    using System.IO;
    using Newtonsoft.Json.Serialization;

    public class NewtonsoftJsonSerializerTests
    {
        private readonly ILogger logger;
        private readonly JsonSerializerSettings settings;
        private readonly NewtonsoftJsonSerializer sut;

        public NewtonsoftJsonSerializerTests()
        {
            logger = A.Fake<ILogger>();
            settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            sut = new NewtonsoftJsonSerializer(settings, logger);
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
            public void OnError_WhenHasMemeberName_ThrowsModelValidationException()
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
            public void OnError_HasMemberValidationForEachInvalidMember()
            {
                /* Arrange */
                using var ms = new MemoryStream();
                using var sw = new StreamWriter(ms);
                sw.Write("{\"foo\":\"foo\",\"bar\":\"lorem\"}");
                sw.Flush();
                ms.Position = 0;

                /* Act */
                var ex = Record.Exception(() => sut.Deserialize<TestModel>(ms.GetBuffer()));
                var error = Assert.IsType<ModelValidationException>(ex);

                /* Assert */
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

            public class OnError : NewtonsoftJsonSerializerTests
            {
                LogEntry entry;

                public OnError()
                {
                    A.CallTo(() => logger.Log(A<LogEntry>._))
                        .Invokes(ctx => entry = ctx.GetArgument<LogEntry>(0));
                }

                [Fact]
                public void LogsErrorSeverity()
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
                    Assert.Equal(LogSeverity.Error, entry.Severity);
                }

                [Fact]
                public void LogsErrorMsg()
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
                    Assert.Equal("Deserialization failed", entry.Message);
                }

                [Fact]
                public void LogsOriginalError()
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
                    Assert.IsType<JsonReaderException>(entry.Exception);
                }

                [Fact]
                public void LogsOriginalObject()
                {
                    /* Arrange */
                    using var ms = new MemoryStream();
                    using var sw = new StreamWriter(ms);
                    sw.Write("{\"bar\":1,\"foo\":\"foo\"}");
                    sw.Flush();
                    ms.Position = 0;

                    /* Act */
                    var ex = Record.Exception(() => sut.Deserialize<TestModel>(ms.GetBuffer()));

                    /* Assert */
                    var model = Assert.IsType<TestModel>(entry.Data);
                    Assert.Equal(1, model.Bar);
                }
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
