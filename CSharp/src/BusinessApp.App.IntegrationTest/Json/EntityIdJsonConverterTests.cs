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

    public class EntityIdJsonConverterTests
    {
        private readonly EntityIdJsonConverter<int> sut;

        public EntityIdJsonConverterTests()
        {
            sut = new EntityIdJsonConverter<int>();
        }

        public class ReadJson : EntityIdJsonConverterTests
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
            [InlineData("true", "Boolean")]
            [InlineData("'2010-01-01T11:01:11'", "DateTime")]
            [InlineData("1.1", "Double")]
            [InlineData("2147483648", "Int64")] // number too big
            public void CannotReadValue_FormatExceptionThrown(string value,
                string wrongTypeName)
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
                        typeof(EntityIntIdStub),
                        A.Dummy<object>(),
                        serializer));
                }

                /* Assert */
                Assert.IsType<FormatException>(ex);
                Assert.Equal(
                    "Cannot read value for 'id' because the type is incorrect. " +
                    $"Expected a 'Int32', but read a '{wrongTypeName}'.",
                    ex.Message
                );
            }

            [Fact]
            public void CannotReadConvertableValue_FormatExceptionThrown()
            {
                /* Arrange */
                string json = "{'id': 'SEkdUlMdVlcA' }";
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
                        typeof(EntityIntIdStub),
                        A.Dummy<object>(),
                        serializer));
                }

                /* Assert */
                Assert.IsType<FormatException>(ex);
                Assert.Equal(
                    "Cannot read value for 'id' because the type is incorrect. Expected " +
                    "a 'Int32'.",
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
                Assert.Equal(new EntityIntIdStub { Id = 11 }, readObj);
            }
        }

        public class WriteJson : EntityIdJsonConverterTests
        {
            [Fact]
            public void ConverterCanConvertToGenericType_WritesJson()
            {
                /* Arrange */
                var serializer = new JsonSerializer();
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);

                /* Act */
                using (var writer = new JsonTextWriter(sw))
                {
                    sut.WriteJson(writer,
                        new EntityIntIdStub() { Id = 1 },
                        serializer);
                }

                /* Assert */
                Assert.Equal("1", sb.ToString());
            }

            [Fact]
            public void ConverterCannotConvertToGenericType_ExceptionThrown()
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
                        new EntityBoolIdStub() { Id = true },
                        serializer));
                }

                /* Assert */
                Assert.IsType<NotSupportedException>(ex);
                Assert.Equal(
                    $"Cannot write the EntityId to a json value. Cannot convert " +
                    "from 'EntityBoolIdStub' to 'Int32'.",
                    ex.Message
                );
            }
        }

        [TypeConverter(typeof(EntityIdTypeConverter<EntityIntIdStub, int>))]
        private sealed class EntityIntIdStub : EntityId<int>
        {
        }

        [TypeConverter(typeof(EntityIdTypeConverter<EntityBoolIdStub, bool>))]
        private sealed class EntityBoolIdStub : EntityId<bool>
        {
        }
    }
}
