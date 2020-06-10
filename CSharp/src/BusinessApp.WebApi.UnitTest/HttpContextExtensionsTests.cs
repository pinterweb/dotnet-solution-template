namespace BusinessApp.WebApi.UnitTest
{
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using Xunit;
    using BusinessApp.WebApi;
    using System.IO;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Buffers;
    using System.Threading;

    public class HttpContextExtensionsTests
    {
        private HttpContext context;

        public HttpContextExtensionsTests()
        {
            context = HttpContextFakeFactory.New();
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
            public void UnknownMethod_DefaultTReturned()
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns("foo");
                A.CallTo(() => context.Request.ContentLength).Returns(0);

                /* Act */
                var result = context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                Assert.Null(result);
            }

            [Theory]
            [InlineData("GET")]
            [InlineData("get")]
            [InlineData("delete")]
            [InlineData("DELETE")]
            public void GetOrDelete_CaseIgnored(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => context.Request.ContentLength).Returns(0);

                /* Act */
                context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                A.CallTo(() => serializer.Deserialize<QueryStub>(A<Stream>._)).MustHaveHappened();
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public void  GetOrDeleteNoQueryStringOrRouteArgs_NotSerialized(string method)
            {
                /* Arrange */
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => context.Request.ContentLength).Returns(0);

                /* Act */
                context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<object>._)).MustNotHaveHappened();
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public void GetOrDeleteUnknownQueryStringKey_NotSerialized(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?blah=blah"));

                /* Act */
                context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                Assert.Empty(serializedData);
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public void GetOrDeleteInvalidQueryStringKey_NotSerialized(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?blahblah"));

                /* Act */
                context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                Assert.Null(serializedData);
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public void GetOrDeleteOnlyHasQueryString_SerializedAllWithDictionary(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?foo=bar&lorem=ipsum"));

                /* Act */
                context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                Assert.True(serializedData.ContainsKey("foo"));
                Assert.Equal("bar", serializedData["foo"]);
                Assert.True(serializedData.ContainsKey("lorem"));
                Assert.Equal("ipsum", serializedData["lorem"]);
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public void GetOrDeleteOnlyHasRouteArg_SerializedAllWithDictionary(string method)
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
                context.DeserializeInto<QueryStub>(serializer);

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
            public void GetOnlyHasQueryString_ConvertsToType(string queryString,
                string key, Type expectedType)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns("get");
                A.CallTo(() => serializer.Serialize(A<Stream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString(queryString));

                /* Act */
                context.DeserializeInto<QueryStub>(serializer);

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
            public void GetOnlyHasRouteArg_ConvertsToType(string key, Type expectedType, object value)
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
                context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                Assert.IsType(expectedType, serializedData[key]);
            }

            [Theory]
            [InlineData("get")]
            [InlineData("DELETE")]
            public void GetOrDeleteCSVQueryString_SerializedToIEnumerable(string method)
            {
                /* Arrange */
                IDictionary<string, object> serializedData = null;
                A.CallTo(() => context.Request.Method).Returns(method);
                A.CallTo(() => serializer.Serialize(A<MemoryStream>._, A<IDictionary<string, object>>._))
                    .Invokes(ctx => serializedData = ctx.GetArgument<IDictionary<string, object>>(1));
                A.CallTo(() => context.Request.QueryString).Returns(new QueryString("?enumerable=1,2,3"));

                /* Act */
                context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                Assert.Equal(new float[] { 1, 2, 3 }, serializedData["enumerable"]);
            }

            [Fact]
            public void AnyMethodWithBody_SerializedNestedClass()
            {
                /* Arrange */
                var stream = A.Dummy<Stream>();
                var query = A.Fake<QueryStub>();
                A.CallTo(() => context.Request.Body).Returns(stream);
                A.CallTo(() => serializer.Deserialize<QueryStub>(stream))
                    .Returns(query);

                /* Act */
                var returned = context.DeserializeInto<QueryStub>(serializer);

                /* Assert */
                Assert.Same(query, returned);
            }
        }
    }
}
