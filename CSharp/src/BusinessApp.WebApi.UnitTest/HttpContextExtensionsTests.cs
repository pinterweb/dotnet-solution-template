namespace BusinessApp.WebApi.UnitTest
{
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using Xunit;
    using System.IO;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.IO.Pipelines;
    using Xunit.Abstractions;
    using System.ComponentModel;

    public class HttpContextExtensionsTests
    {
        private HttpContext context;
        private CancellationToken token;
        private ITestOutputHelper writer;

        public HttpContextExtensionsTests(ITestOutputHelper writer)
        {
            this.writer = writer;
            context = A.Fake<HttpContext>();
            token = A.Dummy<CancellationToken>();
            A.CallTo(() => context.Request.QueryString).Returns(new QueryString(""));
        }

        public class DeserializeInto : HttpContextExtensionsTests
        {
            private readonly ISerializer serializer;

            public DeserializeInto(ITestOutputHelper writer): base(writer)
            {
                serializer = A.Fake<ISerializer>();
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("get")]
            [InlineData("delete")]
            [InlineData("DELETE")]
            public async Task GetOrDeleteMethod_CaseIgnored(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => context.Request.ContentLength).Returns(0);

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                A.CallTo(() => serializer.Serialize(
                        A<Stream>._,
                        A<IDictionary<string, object>>._))
                    .MustHaveHappenedOnceExactly();
            }

            [Theory]
            [InlineData("get", "?blah=blah")]
            [InlineData("DELETE", "?blah=blah")]
            [InlineData("get", "?blahblah")]
            [InlineData("DELETE", "?blahblah")]
            public async Task GetOrDeleteUnknownQueryStringKey_EmptyDictionarySerialized(
                string method, string queryString)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString(queryString));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Empty(serializedData);
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public async Task GetOrDeleteOnlyHasQueryString_SerializedAllWithDictionary(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?foo=bar&lorem=ipsum"));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.True(serializedData.ContainsKey("foo"));
                Assert.Equal("bar", serializedData["foo"]);
                Assert.True(serializedData.ContainsKey("lorem"));
                Assert.Equal("ipsum", serializedData["lorem"]);
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public async Task GetOrDeleteOnlyHasRouteArg_SerializedAllWithDictionary(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                var routeData = new RouteValueDictionary
                {
                    { "foo", "bar" },
                    { "lorem", "ipsum" }
                };
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.RouteValues).Returns(routeData);

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.True(serializedData.ContainsKey("foo"));
                Assert.Equal("bar", serializedData["foo"]);
                Assert.True(serializedData.ContainsKey("lorem"));
                Assert.Equal("ipsum", serializedData["lorem"]);
            }

            [Theory]
            [InlineData("?foo=bar", "foo", typeof(string))]
            [InlineData("?int=12", "int", typeof(int))]
            [InlineData("?enum=foobar", "enum", typeof(EnumQueryStub))]
            [InlineData("?decimal=1.2", "decimal", typeof(decimal))]
            [InlineData("?bool=true", "bool", typeof(bool))]
            [InlineData("?double=1.2", "double", typeof(double))]
            [InlineData("?dateTime=2020-01-01", "dateTime", typeof(DateTime))]
            public async Task GetOnlyHasQueryString_ConvertsToType(string queryString,
                string key, Type expectedType)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns("get");
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString(queryString));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.IsType(expectedType, serializedData[key]);
            }

            public static IEnumerable<object[]> RouteArguments => new[]
            {
                new object[] { "foo", typeof(string), "bar" },
                new object[] { "int", typeof(int), 12 },
                new object[] { "enum", typeof(EnumQueryStub), EnumQueryStub.Foobar },
                new object[] { "decimal", typeof(decimal), 1.2m },
                new object[] { "bool", typeof(bool), true },
                new object[] { "double", typeof(double), 1.2 },
                new object[] { "dateTime", typeof(DateTime), "2020-01-01" },
            };

            [Theory, MemberData(nameof(RouteArguments))]
            public async Task GetOnlyHasRouteArg_ConvertsToType(string key, Type expectedType, object value)
            {
                /* Arrange */
                var routeData = new RouteValueDictionary
                {
                    { key, value }
                };
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns("get");
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.RouteValues).Returns(routeData);

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.IsType(expectedType, serializedData[key]);
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public async Task GetOrDeleteCSVQueryString_SerializedToIEnumerable(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<MemoryStream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?enumerable=1,2,3"));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Equal(new float[] { 1, 2, 3 }, serializedData["enumerable"]);
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("delete")]
            public async Task GetOrDeleteNestedClassWithDot_SerializedToDictionary(string method)
            {
                /* Arrange */
                var expectNestedDictionary = new Dictionary<string, object>
                {
                    { "ipsit", "blah" },
                    { "dolor", "boo" },
                };
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString)
                    .Returns(new QueryString("?singleNested.ipsit=blah&singleNested.dolor=boo"));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Equal(expectNestedDictionary, serializedData["singleNested"]);
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("delete")]
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
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString)
                    .Returns(new QueryString("?deeplyNested.nested.ipsit=blah&deeplyNested.nested.dolor=boo"));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Equal(expectNestedDictionary, serializedData["deeplyNested"]);
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("delete")]
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
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString)
                    .Returns(new QueryString("?deeplyNested.nested.enumerable=1,2"));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Equal(expectNestedDictionary, serializedData["deeplyNested"]);
            }

            [Fact]
            public async Task NotGetOrDeleteWithBody_BodyDeserialized()
            {
                /* Arrange */
                var reader = A.Dummy<PipeReader>();
                var query = A.Fake<QueryStub>();
                A.CallTo(() => context.Request.BodyReader).Returns(reader);
                A.CallTo(() => serializer.Deserialize<QueryStub>(A<MemoryStream>._))
                    .Returns(query);

                /* Act */
                var returned = await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Same(query, returned);
            }

            [Fact]
            public async Task NotGetOrDeleteWithBodyAndRouteData_AllDeserialized()
            {
                /* Arrange */
                /* Arrange */
                var routeData = new RouteValueDictionary
                {
                    { "foo", "bar" }
                };
                A.CallTo(() => context.Request.RouteValues).Returns(routeData);
                var reader = A.Dummy<PipeReader>();
                var query = new QueryStub { Bool = true, Foo = null };
                A.CallTo(() => context.Request.BodyReader).Returns(reader);
                A.CallTo(() => serializer.Deserialize<QueryStub>(A<MemoryStream>._))
                    .Returns(query);

                /* Act */
                var returned = await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Equal("bar", returned.Foo);
                Assert.True(returned.Bool);
            }
        }
    }
}
