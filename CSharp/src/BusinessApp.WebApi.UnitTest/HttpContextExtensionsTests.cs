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

    public class HttpContextExtensionsTests
    {
        private HttpContext context;
        private CancellationToken token;

        public HttpContextExtensionsTests()
        {
            context = HttpContextFakeFactory.New();
            token = A.Dummy<CancellationToken>();
            A.CallTo(() => context.Request.QueryString).Returns(new QueryString(""));
        }

        public class DeserializeInto : HttpContextExtensionsTests
        {
            private readonly ISerializer serializer;

            public DeserializeInto()
            {
                serializer = A.Fake<ISerializer>();
            }

            [Fact]
            public async Task UnknownMethod_DefaultTReturned()
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns("foo");
                A.CallTo(() => context.Request.ContentLength).Returns(0);

                /* Act */
                var result = await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Null(result);
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("get")]
            [InlineData("delete")]
            [InlineData("DELETE")]
            public async Task GetOrDelete_CaseIgnored(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => context.Request.ContentLength).Returns(0);

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                A.CallTo(() => serializer.Deserialize<QueryStub>(A<Stream>._)).MustHaveHappened();
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public async Task  GetOrDeleteNoQueryStringOrRouteArgs_NotSerialized(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => context.Request.ContentLength).Returns(0);

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<object>._)).MustNotHaveHappened();
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public async Task GetOrDeleteUnknownQueryStringKey_NotSerialized(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?blah=blah"));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Empty(serializedData);
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public async Task GetOrDeleteInvalidQueryStringKey_NotSerialized(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?blahblah"));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Null(serializedData);
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
                // the GetRouteData extension calls this
                A.CallTo(() => context.Features.Get<IRoutingFeature>()).Returns(null);

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

            [Theory]
            [InlineData("foo", typeof(string), "bar")]
            [InlineData("int", typeof(int), 12)]
            [InlineData("enum", typeof(EnumQueryStub), EnumQueryStub.Foobar)]
            [InlineData("decimal", typeof(decimal), 1.2)]
            [InlineData("bool", typeof(bool), true)]
            [InlineData("double", typeof(double), 1.2)]
            [InlineData("dateTime", typeof(DateTime), "2020-01-01")]
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
                // the GetRouteData extension calls this
                A.CallTo(() => context.Features.Get<IRoutingFeature>()).Returns(null);

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

            [Fact]
            public async Task AnyMethodWithBody_SerializedNestedClass()
            {
                /* Arrange - TODO no unit test on pipe reader, do functional test */
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
        }
    }
}
