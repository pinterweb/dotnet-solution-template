namespace BusinessApp.App.IntegrationTest.Json
{
    using System;
    using FakeItEasy;
    using Xunit;
    using BusinessApp.App.Json;
    using Newtonsoft.Json;
    using System.IO;
    using System.Text;

    public class NewtonsoftLongToStringJsonConverterTests
    {
        private readonly NewtonsoftLongToStringJsonConverter sut;

        public NewtonsoftLongToStringJsonConverterTests()
        {
            sut = new NewtonsoftLongToStringJsonConverter();
        }

        public class CanConvert : NewtonsoftLongToStringJsonConverterTests
        {
            [Theory]
            [InlineData(typeof(long), true)]
            [InlineData(typeof(string), false)]
            public void LongType(Type objectType, bool expectCanConvert)
            {
                /* Act */
                var canConvert = sut.CanConvert(objectType);

                /* Assert */
                Assert.Equal(expectCanConvert, canConvert);
            }
        }

        public class ReadJson : NewtonsoftLongToStringJsonConverterTests
        {
            [Fact]
            public void NullToken_NullReturned()
            {
                /* Arrange */
                var reader = A.Fake<JsonReader>();
                var serializer = A.Dummy<JsonSerializer>();
                A.CallTo(() => reader.TokenType).Returns(JsonToken.Null);

                /* Act */
                var readObj = sut.ReadJson(reader,
                    A.Dummy<Type>(),
                    A.Dummy<object>(),
                    serializer);

                /* Assert */
                Assert.Null(readObj);
            }

            [Fact]
            public void LongAsStringValue_LongReturned()
            {
                /* Arrange */
                var reader = A.Fake<JsonReader>();
                var serializer = A.Dummy<JsonSerializer>();
                A.CallTo(() => reader.Value).Returns("123");

                /* Act */
                var readObj = sut.ReadJson(reader,
                    A.Dummy<Type>(),
                    A.Dummy<object>(),
                    serializer);

                /* Assert */
                var longVal = Assert.IsType<long>(readObj);
                Assert.Equal(123, longVal);
            }
        }

        public class WriteJson : NewtonsoftLongToStringJsonConverterTests
        {
            [Fact]
            public void NullType_WritesAsNull()
            {
                /* Arrange */
                StringBuilder sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using (var writer = new JsonTextWriter(sw))
                {

                    var serializer = A.Fake<JsonSerializer>();
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");

                    /* Act */
                    sut.WriteJson(writer, null, serializer);
                    writer.WriteEnd();
                }

                /* Assert */
                Assert.Equal("{\"id\":null}", sb.ToString());
            }

            [Fact]
            public void LongType_WritesAsString()
            {
                /* Arrange */
                StringBuilder sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using (var writer = new JsonTextWriter(sw))
                {

                    var serializer = A.Fake<JsonSerializer>();
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");

                    /* Act */
                    sut.WriteJson(writer, 11, serializer);
                    writer.WriteEnd();
                }

                /* Assert */
                Assert.Equal("{\"id\":\"11\"}", sb.ToString());
            }
        }
    }
}
