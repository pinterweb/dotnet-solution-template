namespace BusinessApp.App.IntegrationTest.Json
{
    using System;
    using FakeItEasy;
    using Xunit;
    using BusinessApp.App.Json;
    using Newtonsoft.Json;
    using BusinessApp.Domain;
    using System.IO;
    using System.ComponentModel;
    using System.Text;
    using BusinessApp.Test;

    public class NewtonsoftEntityIdJsonConverterTests
    {
        private readonly NewtonsoftEntityIdJsonConverter sut;

        public NewtonsoftEntityIdJsonConverterTests()
        {
            sut = new NewtonsoftEntityIdJsonConverter();
        }

        public class CanConvert : NewtonsoftEntityIdJsonConverterTests
        {
            [Theory]
            [InlineData(typeof(EntityIdStub), true)]
            [InlineData(typeof(string), false)]
            public void LongType(Type objectType, bool expectCanConvert)
            {
                /* Act */
                var canConvert = sut.CanConvert(objectType);

                /* Assert */
                Assert.Equal(expectCanConvert, canConvert);
            }
        }

        public class ReadJson : NewtonsoftEntityIdJsonConverterTests
        {
            [Fact]
            public void NullToken_NullReturned()
            {
                /* Arrange */
                var reader = A.Fake<JsonReader>();
                A.CallTo(() => reader.TokenType).Returns(JsonToken.Null);

                /* Act */
                var readObj = sut.ReadJson(reader, A.Dummy<Type>(), A.Dummy<object>(), A.Dummy<JsonSerializer>());

                /* Assert */
                Assert.Null(readObj);
            }

            [Fact]
            public void NullValue_NullReturned()
            {
                /* Arrange */
                var reader = A.Fake<JsonReader>();
                var serializer = A.Fake<JsonSerializer>();
                A.CallTo(() => reader.Value).Returns(null);

                /* Act */
                var readObj = sut.ReadJson(reader,
                    A.Dummy<Type>(),
                    A.Dummy<object>(),
                    serializer);

                /* Assert */
                Assert.Null(readObj);
            }

            [Theory]
            [InlineData(typeof(EntityIntIdStub), "true")]
            [InlineData(typeof(EntityIntIdStub), "'2010-01-01T11:01:11'")]
            [InlineData(typeof(EntityIntIdStub), "1.1")]
             // number too big, but will attempt to convert though. All json numbers are long
            [InlineData(typeof(EntityIntIdStub), "2147483648")]
            [InlineData(typeof(NullableEntityIntIdStub), "2147483648")]
            public void CannotReadValue_AppExceptionThrown(Type idType, string value)
            {
                /* Arrange */
                string json = "{'id': " + value + " }";
                var serializer = new JsonSerializer();
                Exception ex = null;

                /* Act */
                using (var reader = new JsonTextReader(new StringReader(json)))
                {
                    // Start of object, then property name, then value
                    reader.Read();
                    reader.Read();
                    reader.Read();

                    ex = Record.Exception(() => sut.ReadJson(reader,
                        idType,
                        A.Dummy<object>(),
                        serializer));
                }

                /* Assert */
                Assert.IsType<BusinessAppAppException>(ex);
                Assert.Equal(
                    "Cannot read value for 'id' because the type is incorrect",
                    ex.Message
                );
            }

            [Fact]
            public void CustomConverterCanReadValue_EntityIdReturned()
            {
                /* Arrange */
                string json = @"{'id': '11'}";
                var serializer = new JsonSerializer();
                object readObj = null;

                /* Act */
                using (var reader = new JsonTextReader(new StringReader(json)))
                {
                    // Start of object, then property, then value
                    reader.Read();
                    reader.Read();
                    reader.Read();

                    readObj = sut.ReadJson(reader,
                        typeof(EntityIntIdStub),
                        A.Dummy<object>(),
                        serializer);
                }

                /* Assert */
                Assert.Equal(new EntityIntIdStub(11), readObj);
            }
        }

        public class WriteJson : NewtonsoftEntityIdJsonConverterTests
        {
            [Fact]
            public void IsAssignableFromIEntity_WritesJson()
            {
                /* Arrange */
                var serializer = new JsonSerializer();
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);

                /* Act */
                using (var writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Id");
                    sut.WriteJson(writer,
                        new EntityIntIdStub(1),
                        serializer);
                    writer.WriteEndObject();
                }

                /* Assert */
                Assert.Equal("{\"Id\":1}", sb.ToString());
            }

            [Fact]
            public void IsNotAssignableFromIEntity_ExceptionThrown()
            {
                /* Arrange */
                var serializer = new JsonSerializer();
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                Exception ex = null;

                /* Act */
                using (var writer = new JsonTextWriter(sw))
                {
                    ex = Record.Exception(() => sut.WriteJson(writer,
                        true,
                        serializer));
                }

                /* Assert */
                Assert.IsType<NotSupportedException>(ex);
                Assert.Equal(
                    "Cannot write the json value because the source value is not an IEntityId",
                    ex.Message
                );
            }
        }

        [TypeConverter(typeof(EntityIdTypeConverter<EntityIntIdStub, int>))]
        private class NullableEntityIntIdStub : IEntityId
        {
            public NullableEntityIntIdStub(int id)
            {
                Id = id;
            }

            public int? Id { get; set; }
            public TypeCode GetTypeCode() => TypeCode.Int32;

            string IConvertible.ToString(IFormatProvider provider) => Id?.ToString();
            int IConvertible.ToInt32(IFormatProvider provider) => Id ?? 0;
        }

        [TypeConverter(typeof(EntityIdTypeConverter<EntityIntIdStub, int>))]
        private struct EntityIntIdStub : IEntityId
        {
            public EntityIntIdStub(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
            public TypeCode GetTypeCode() => TypeCode.Int32;

            int IConvertible.ToInt32(IFormatProvider provider) => Id;
            string IConvertible.ToString(IFormatProvider provider) => Id.ToString();
        }

        [TypeConverter(typeof(EntityIdTypeConverter<EntityBoolIdStub, bool>))]
        private struct EntityBoolIdStub : IEntityId
        {
            public EntityBoolIdStub(bool id)
            {
                Id = id;
            }

            public bool Id { get; set; }
            public TypeCode GetTypeCode() => TypeCode.Boolean;

            bool IConvertible.ToBoolean(IFormatProvider provider) => Id;
            string IConvertible.ToString(IFormatProvider provider) => Id.ToString();
        }
    }
}
