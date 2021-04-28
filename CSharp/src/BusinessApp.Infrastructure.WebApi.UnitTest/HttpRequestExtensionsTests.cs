using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace BusinessApp.Infrastructure.WebApi.UnitTest
{
    using System.Collections.Generic;
    using System;
    using BusinessApp.Kernel;
    using System.ComponentModel;

    public enum DummyEnum { Foobar }

    public class NestedDummy
    {
        public string Ipsit { get; set; }
        public string Dolor { get; set; }
        public IEnumerable<float> Enumerable { get; set; }
    }

    public class DeeplyNestedQueryStub
    {
        public NestedDummy Nested { get; set; }
    }

    [TypeConverter(typeof(EntityIdTypeConverter<DummyId, int>))]
    public class DummyId : IEntityId
    {
        public DummyId(int id)
        {
            Id = id;
        }
        public int Id { get; set; }
        public TypeCode GetTypeCode() => Type.GetTypeCode(typeof(int));
    }

    public class Dummy
    {
        public int ReadOnlyId { get; }
        public DummyId Id { get; set; }
        public bool? Bool { get; set; }
        public int? Int { get; set; }
        public decimal? Decimal { get; set; }
        public double? Double { get; set; }
        public string Foo { get; set; }
        public string Lorem { get; set; }
        public DateTime? DateTime { get; set; }
        public DummyEnum? Enum { get; set; }
        public NestedDummy SingleNested { get; set; }
        public DeeplyNestedQueryStub DeeplyNested { get; set; }
        public IEnumerable<NestedDummy> MultiNested { get; set; }
        public IEnumerable<float> Enumerable { get; set; }
    }

    public class HttpRequestExtensionsTests
    {
        private HttpRequest sut;

        public HttpRequestExtensionsTests()
        {
            sut = A.Fake<HttpRequest>();
            A.CallTo(() => sut.QueryString).Returns(new QueryString(""));
        }

        public class DeserializeAsync : HttpRequestExtensionsTests
        {
            private readonly ISerializer serializer;

            public DeserializeAsync()
            {
                serializer = A.Fake<ISerializer>();
            }

            public static IEnumerable<object[]> QueryMethods => new[]
            {
                new object[] { HttpMethods.Get },
                new object[] { HttpMethods.Delete }
            };

            public static IEnumerable<object[]> SaveMethods => new[]
            {
                new object[] { HttpMethods.Post },
                new object[] { HttpMethods.Put },
                new object[] { HttpMethods.Patch }
            };


            public static IEnumerable<object[]> AllMethods => SaveMethods.Concat(QueryMethods);

            [Theory, MemberData(nameof(AllMethods))]
            public async Task NotQueryStringRouteArgsOrBody_EmptyDictionarySerialized(string method)
            {
                /* Arrange */
                IDictionary<string, object> payload = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => sut.ContentLength).Returns(0);
                A.CallTo(() => serializer.Serialize<IDictionary<string, object>>(A<IDictionary<string, object>>._))
                    .Invokes(c => payload = c.GetArgument<IDictionary<string, object>>(0));

                /* Act */
                var result = await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                Assert.NotNull(payload);
                Assert.Empty(payload);
            }

            [Theory, MemberData(nameof(AllMethods))]
            public async Task NotQueryStringRouteArgsOrBody_NotSerialized(string method)
            {
                /* Arrange */
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => sut.ContentLength).Returns(0);

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                A.CallTo(() => serializer.Serialize(A<object>._)).MustNotHaveHappened();
            }

            public static IEnumerable<object[]> BadQueryStrings => new[]
            {
                new object[] { HttpMethods.Get, "?blah=blah" },
                new object[] { HttpMethods.Get, "?blahblah" },
                new object[] { HttpMethods.Delete, "?blah=blah" },
                new object[] { HttpMethods.Delete, "?blahblah" },
            };

            [Theory, MemberData(nameof(BadQueryStrings))]
            public async Task GetOrDeleteUnknownQueryStringKey_NotSerialized(
                string method, string queryString)
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => sut.QueryString).Returns(new QueryString(queryString));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, token);

                /* Assert */
                A.CallTo(() => serializer.Serialize(A<object>._)).MustNotHaveHappened();
            }

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteOnlyHasQueryString_SerializedAllWithDictionary(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.QueryString).Returns(new QueryString("?foo=bar&lorem=ipsum"));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                Assert.True(serializedData.ContainsKey("foo"));
                Assert.Equal("bar", serializedData["foo"]);
                Assert.True(serializedData.ContainsKey("lorem"));
                Assert.Equal("ipsum", serializedData["lorem"]);
            }

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteHasAggregateIdAsArg_ValueTypeSerialized(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.QueryString).Returns(new QueryString("?id=1"));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                var id = Assert.IsType<DummyId>(serializedData["id"]);
                Assert.Equal(1, id.Id);
            }

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteOnlyHasRouteArg_SerializedAllWithDictionary(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                var routeData = new RouteValueDictionary
                {
                    { "foo", "bar" },
                    { "lorem", "ipsum" }
                };
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.RouteValues).Returns(routeData);

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                Assert.True(serializedData.ContainsKey("foo"));
                Assert.Equal("bar", serializedData["foo"]);
                Assert.True(serializedData.ContainsKey("lorem"));
                Assert.Equal("ipsum", serializedData["lorem"]);
            }

            [Theory]
            [InlineData("?foo=bar", "foo", typeof(string))]
            [InlineData("?int=12", "int", typeof(int))]
            [InlineData("?enum=foobar", "enum", typeof(DummyEnum))]
            [InlineData("?decimal=1.2", "decimal", typeof(decimal))]
            [InlineData("?bool=true", "bool", typeof(bool))]
            [InlineData("?double=1.2", "double", typeof(double))]
            [InlineData("?dateTime=2020-01-01", "dateTime", typeof(DateTime))]
            public async Task GetOnlyHasQueryString_ConvertsToType(string queryString,
                string key, Type expectedType)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(HttpMethods.Get);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.QueryString).Returns(new QueryString(queryString));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                Assert.IsType(expectedType, serializedData[key]);
            }

            public static IEnumerable<object[]> RouteArguments => new[]
            {
                new object[] { "foo", typeof(string), "bar" },
                new object[] { "int", typeof(int), 12 },
                new object[] { "enum", typeof(DummyEnum), DummyEnum.Foobar },
                new object[] { "decimal", typeof(decimal), 1.2m },
                new object[] { "bool", typeof(bool), true },
                new object[] { "double", typeof(double), 1.2 },
                new object[] { "dateTime", typeof(DateTime), "2020-01-01" },
            };

            [Theory, MemberData(nameof(RouteArguments))]
            public async Task GetOnlyHasRouteArg_ConvertsToType(string key, Type expectedType,
                object value)
            {
                /* Arrange */
                var routeData = new RouteValueDictionary
                {
                    { key, value }
                };
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(HttpMethods.Get);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.RouteValues).Returns(routeData);

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                Assert.IsType(expectedType, serializedData[key]);
            }

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteHasRouteArgAndQueryString_SerializedBoth(string method)
            {
                /* Arrange */
                var routeData = new RouteValueDictionary
                {
                    { "bool", true }
                };
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.RouteValues).Returns(routeData);
                A.CallTo(() => sut.QueryString).Returns(new QueryString("?foo=bar"));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                Assert.Equal("bar", serializedData["foo"]);
                Assert.Equal(true, serializedData["bool"]);
            }

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteCSVQueryString_SerializedToIEnumerable(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.QueryString).Returns(new QueryString("?enumerable=1,2,3"));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                Assert.Equal(new float[] { 1, 2, 3 }, serializedData["enumerable"]);
            }

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteNestedClassWithDot_SerializedToDictionary(string method)
            {
                /* Arrange */
                var expectNestedDictionary = new Dictionary<string, object>
                {
                    { "ipsit", "blah" },
                    { "dolor", "boo" },
                };
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.QueryString)
                    .Returns(new QueryString("?singleNested.ipsit=blah&singleNested.dolor=boo"));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, default);

                /* Assert */
                Assert.Equal(expectNestedDictionary, serializedData["singleNested"]);
            }

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteDeeplyNestedClassWithDot_SerializedToDictionary(string method)
            {
                /* Arrange */
                var expectNestedDictionary = new Dictionary<string, object>
                {
                    {
                        "nested",
                        new Dictionary<string, object>
                        {
                            { "ipsit", "blah" },
                            { "dolor", "boo" },
                        }
                    }
                };
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.QueryString)
                    .Returns(new QueryString("?deeplyNested.nested.ipsit=blah&deeplyNested.nested.dolor=boo"));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(expectNestedDictionary, serializedData["deeplyNested"]);
            }

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteDeeplyNestedEnumerableWithDot_SerializedToDictionary(string method)
            {
                /* Arrange */
                var expectNestedDictionary = new Dictionary<string, object>
                {
                    {
                        "nested",
                        new Dictionary<string, object>
                        {
                            { "enumerable", new float[] { 1, 2 } }
                        }
                    }
                };
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(0));
                A.CallTo(() => sut.QueryString)
                    .Returns(new QueryString("?deeplyNested.nested.enumerable=1,2"));

                /* Act */
                await sut.DeserializeAsync<Dummy>(serializer, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(expectNestedDictionary, serializedData["deeplyNested"]);
            }

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotGetOrDeleteWithNullModel_NullReturned(string method)
            {
                /* Arrange */
                var bytes = Encoding.UTF8.GetBytes("foobarish");
                var result = new ValueTask<ReadResult>(
                    new ReadResult(new ReadOnlySequence<byte>(bytes), true, true));
                var token = A.Dummy<CancellationToken>();
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => sut.BodyReader.ReadAsync(token))
                    .Returns(result);
                A.CallTo(() => serializer.Deserialize<Dummy>(A<byte[]>._)).Returns(null);

                /* Act */
                var returned = await sut.DeserializeAsync<Dummy>(serializer, token);

                /* Assert */
                Assert.Null(returned);
            }

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotGetOrDeleteWithBody_BodyDeserialized(string method)
            {
                /* Arrange */
                var bytes = Encoding.UTF8.GetBytes("foobarish");
                var result = new ValueTask<ReadResult>(
                    new ReadResult(new ReadOnlySequence<byte>(bytes), true, true));
                var token = A.Dummy<CancellationToken>();
                byte[] buffer = null;
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => sut.BodyReader.ReadAsync(token))
                    .Returns(result);
                A.CallTo(() => serializer.Deserialize<Dummy>(A<byte[]>._))
                    .Invokes(ctx => buffer = ctx.GetArgument<byte[]>(0));

                /* Act */
                var returned = await sut.DeserializeAsync<Dummy>(serializer, token);

                /* Assert */
                Assert.Equal(bytes, buffer);
            }

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotGetOrDeleteWithBodyAndRouteData_BothDeserialized(string method)
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var routeData = new RouteValueDictionary
                {
                    { "foo", "bar" }
                };
                var result = new ValueTask<ReadResult>(
                    new ReadResult(new ReadOnlySequence<byte>(), true, true));
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => sut.RouteValues).Returns(routeData);
                A.CallTo(() => sut.BodyReader.ReadAsync(token))
                    .Returns(result);
                var query = new Dummy { Bool = true, Foo = null };
                A.CallTo(() => serializer.Deserialize<Dummy>(A<byte[]>._))
                    .Returns(query);

                /* Act */
                var returned = await sut.DeserializeAsync<Dummy>(serializer,
                    A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal("bar", returned.Foo);
                Assert.True(returned.Bool);
            }

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotGetOrDeleteWithBodyAndRouteData_WhenNullRouteValue_NullSet(string method)
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var routeData = new RouteValueDictionary
                {
                    { "foo", null }
                };
                var result = new ValueTask<ReadResult>(
                    new ReadResult(new ReadOnlySequence<byte>(), true, true));
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => sut.RouteValues).Returns(routeData);
                A.CallTo(() => sut.BodyReader.ReadAsync(token))
                    .Returns(result);
                var query = new Dummy { Bool = true, Foo = null };
                A.CallTo(() => serializer.Deserialize<Dummy>(A<byte[]>._))
                    .Returns(query);

                /* Act */
                var returned = await sut.DeserializeAsync<Dummy>(serializer,
                    A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Null(returned.Foo);
            }

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotGetOrDeleteWithBody_WhenReadOnlyProperty_Ignored(string method)
            {
                /* Arrange */
                var token = A.Dummy<CancellationToken>();
                var routeData = new RouteValueDictionary
                {
                    { "ReadOnlyId", 1 }
                };
                var result = new ValueTask<ReadResult>(
                    new ReadResult(new ReadOnlySequence<byte>(), true, true));
                A.CallTo(() => sut.Method).Returns(method);
                A.CallTo(() => sut.RouteValues).Returns(routeData);
                A.CallTo(() => sut.BodyReader.ReadAsync(token))
                    .Returns(result);
                var query = new Dummy { Bool = true, Foo = null };
                A.CallTo(() => serializer.Deserialize<Dummy>(A<byte[]>._))
                    .Returns(query);

                /* Act */
                var returned = await sut.DeserializeAsync<Dummy>(serializer,
                    A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(0, returned.ReadOnlyId);
            }
        }
    }
}
