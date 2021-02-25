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

        public class WriteJson : NewtonsoftLongToStringJsonConverterTests
        {
            [Fact]
            public void LongTypeIsString()
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
