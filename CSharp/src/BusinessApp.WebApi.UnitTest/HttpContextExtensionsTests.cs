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
    using Xunit.Abstractions;
    using System.Linq;

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
            public async Task NoQueryStringRouteArgsOrBody_NewInstanceStillReturned(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => context.Request.ContentLength).Returns(0);

                /* Act */
                var result = await context.DeserializeIntoAsync<QueryStub>(serializer, default);

                /* Assert */
                Assert.NotNull(result);
            }

            public static IEnumerable<object[]> BadQueryStrings => new[]
            {
                new object[] { HttpMethods.Get, "?blah=blah" },
                new object[] { HttpMethods.Get, "?blahblah" },
                new object[] { HttpMethods.Delete, "?blah=blah" },
                new object[] { HttpMethods.Delete, "?blahblah" },
            };

            [Theory, MemberData(nameof(BadQueryStrings))]
            public async Task GetOrDeleteUnknownQueryStringKey_NewInstanceStillReturned(
                string method, string queryString)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString(queryString));

                /* Act */
                var result = await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.NotNull(result);
            }

            [Theory, MemberData(nameof(QueryMethods))]
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

            [Theory, MemberData(nameof(QueryMethods))]
            public async Task GetOrDeleteHasRouteArgAndQueryString_SerializedBoth(string method)
            {
                /* Arrange */
                var routeData = new RouteValueDictionary
                {
                    { "bool", true }
                };
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.RouteValues).Returns(routeData);
                // the GetRouteData extension calls this
                A.CallTo(() => context.Features.Get<IRoutingFeature>()).Returns(null);
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?foo=bar"));

                /* Act */
                await context.DeserializeIntoAsync<QueryStub>(serializer, default);

                /* Assert */
                Assert.Equal("bar", serializedData["foo"]);
                Assert.Equal(true, serializedData["bool"]);
            }

            [Theory, MemberData(nameof(QueryMethods))]
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

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotGetOrDeleteWithBody_BodyDeserialized(string method)
            {
                /* Arrange */
                var query = A.Fake<QueryStub>();
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Deserialize<QueryStub>(context.Request.Body))
                    .Returns(query);

                /* Act */
                var returned = await context.DeserializeIntoAsync<QueryStub>(serializer, token);

                /* Assert */
                Assert.Same(query, returned);
            }

            [Theory, MemberData(nameof(SaveMethods))]
            public async Task NotGetOrDeleteWithBodyAndRouteData_BothDeserialized(string method)
            {
                /* Arrange */
                var routeData = new RouteValueDictionary
                {
                    { "foo", "bar" }
                };
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => context.Request.RouteValues).Returns(routeData);
                var query = new QueryStub { Bool = true, Foo = null };
                A.CallTo(() => serializer.Deserialize<QueryStub>(context.Request.Body))
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
