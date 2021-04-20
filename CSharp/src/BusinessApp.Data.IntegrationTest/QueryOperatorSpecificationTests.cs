using System;
using System.Collections.Generic;
using BusinessApp.Infrastructure;
using Xunit;

namespace BusinessApp.Data.IntegrationTest
{
    public class QueryOperatorSpecificationQueryTests
    {
        private static readonly DateTime DateForTests = DateTime.Now;
        private readonly QueryOperatorSpecificationBuilder<QueryWithOperators, ResponseFromOperators> sut;
        private readonly ResponseFromOperators response;
        private readonly QueryWithOperators query;

        public QueryOperatorSpecificationQueryTests()
        {
            sut = new QueryOperatorSpecificationBuilder<QueryWithOperators, ResponseFromOperators>();
            response = new ResponseFromOperators();
            query = new QueryWithOperators();
        }

        public static IEnumerable<object[]> Dates  => new[]
        {
            new object[] { DateForTests, DateForTests },
            new object[] { DateForTests, DateTime.Now.AddDays(-1) },
            new object[] { DateForTests, DateTime.Now.AddDays(1) }
        };

        public static IEnumerable<object[]> EqualStrings  => new[]
        {
            new object[] { "foo", "foo", true },
            new object[] { "foo", "bar", false }
        };

        public static IEnumerable<object[]> StartsWithStrings  => new[]
        {
            new object[] { "foo", "foobarish", true },
            new object[] { "foo", "barfoo", false }
        };

        public static IEnumerable<object[]> Integers  => new[]
        {
            new object[] { 1, 1 },
            new object[] { 1, 2 },
            new object[] { 1, 0 }
        };

        public static IEnumerable<object[]> Decimals  => new[]
        {
            new object[] { 1m, 1m },
            new object[] { 1m, 2m },
        };

        [Theory, MemberData(nameof(Decimals))]
        public void OneDecimalProperty_NotEqual_ContractIsSatisfied(decimal a,
            decimal b)
        {
            /* Arrange */
            query.Decimal_Neq = a;
            response.DecimalVal = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(b != a, result);
        }

        [Theory, MemberData(nameof(Integers))]
        public void OneIntProperty_LessThan_ContractIsSatisfied(int a,
            int b)
        {
            /* Arrange */
            query.OneInt_Lt = a;
            response.OneInt = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(b < a, result);
        }

        [Theory, MemberData(nameof(Integers))]
        public void OneIntProperty_GreaterThan_ContractIsSatisfied(int a,
            int b)
        {
            /* Arrange */
            query.OneInt_Gt = a;
            response.OneInt = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(b > a, result);
        }

        [Theory, MemberData(nameof(Dates))]
        public void OneDateProperty_WhenEqual_ContractIsSatisfied(DateTime a,
            DateTime b)
        {
            /* Arrange */
            query.OneDate_Eq = a;
            response.DateVal = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(a == b, result);
        }

        [Theory, MemberData(nameof(Dates))]
        public void OneDateProperty_WhenLessThanOrEqualTo_ContractIsSatisfied(DateTime a,
            DateTime b)
        {
            /* Arrange */
            query.OneDate_Lte = a;
            response.DateVal = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(b <= a, result);
        }

        [Theory, MemberData(nameof(Dates))]
        public void StartDateProperty_GreaterThanOrEqualTo_ContractIsSatisfied(DateTime a,
            DateTime b)
        {
            /* Arrange */
            query.OneDate_Gte = a;
            response.DateVal = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(b >= a, result);
        }


        [Theory, MemberData(nameof(EqualStrings))]
        public void ManyStringsProperty_WhenContractContainsValue_ContractIsSatisfied(string a,
            string b, bool expect)
        {
            /* Arrange */
            query.ManyString_Contains = new[] { a };
            response.StringVal = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(expect, result);
        }

        [Fact]
        public void NullableProperty_WhenContractContainsValue_DoesNotThrowCoercionError()
        {
            /* Arrange */
            query.ManyInts_Contains = new int?[] { 1 };
            response.OneNullableInt = 1;
            var spec = sut.Build(query);

            /* Act */
            var ex = Record.Exception(() => spec.IsSatisfiedBy(response));

            /* Assert */
            Assert.Null(ex);
        }

        [Theory, MemberData(nameof(StartsWithStrings))]
        public void StringsProperty_WhenContractStartsWith_ContractIsSatisfied(string a,
            string b, bool expect)
        {
            /* Arrange */
            query.String_Sw = a;
            response.StringVal = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(expect, result);
        }

        [Theory, MemberData(nameof(Integers))]
        public void OneIntNestedQueryProperty_LessThan_NestedContractIsSatisfied(int a,
            int b)
        {
            /* Arrange */
            query.NestedQuery = new NestedQueryWithOperators { OneInt_Lt = a };
            response.NestedResponse = new NestedResponseFromOperators { OneInt = b };
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(b < a, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ChildQuery_EqualBool_ContractIsSatisfied(bool responseVal)
        {
            /* Arrange */
            var childQuery = new ChildQueryWithOperators { OneBool = true };
            response.BoolVal = responseVal;
            var spec = sut.Build(childQuery);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(responseVal, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DeeplyNestedQuery_EqualBool_ContractIsSatisfied(bool responseVal)
        {
            /* Arrange */
            var query = new QueryWithOperators
            {
                NestedQuery = new NestedQueryWithOperators
                {
                    AnotherNestedQuery = new AnotherNestedQueryWithOperators
                    {
                        BoolVal = true
                    }
                }
            };
            response.NestedResponse = new NestedResponseFromOperators
            {
                AnotherNestedResponse = new AnotherNestedResponseFromOperators
                {
                    BoolVal = responseVal
                }
            };
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(responseVal, result);
        }

        private sealed class ChildQueryWithOperators : QueryWithOperators
        {
            [QueryOperator(nameof(ResponseFromOperators.BoolVal), QueryOperators.Equal)]
            public bool? OneBool { get; set; }
        }

        private class QueryWithOperators
        {
            [QueryOperator(nameof(ResponseFromOperators.DateVal), QueryOperators.Equal)]
            public DateTime? OneDate_Eq { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.DateVal), QueryOperators.LessThanOrEqualTo)]
            public DateTime? OneDate_Lte { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.DateVal), QueryOperators.GreaterThanOrEqualTo)]
            public DateTime? OneDate_Gte { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.OneInt), QueryOperators.LessThan)]
            public int? OneInt_Lt { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.OneInt), QueryOperators.GreaterThan)]
            public int? OneInt_Gt { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.DecimalVal), QueryOperators.NotEqual)]
            public decimal? Decimal_Neq { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.StringVal), QueryOperators.StartsWith)]
            public string String_Sw { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.StringVal), QueryOperators.Contains)]
            public IEnumerable<string> ManyString_Contains { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.OneNullableInt), QueryOperators.Contains)]
            public IEnumerable<int?> ManyInts_Contains { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.NestedResponse))]
            public NestedQueryWithOperators NestedQuery { get; set; }
        }

        private sealed class NestedQueryWithOperators
        {
            [QueryOperator(nameof(NestedResponseFromOperators.OneInt), QueryOperators.LessThan)]
            public int? OneInt_Lt { get; set; }

            [QueryOperator(nameof(NestedResponseFromOperators.AnotherNestedResponse))]
            public AnotherNestedQueryWithOperators AnotherNestedQuery { get; set; }
        }

        private sealed class AnotherNestedQueryWithOperators
        {
            [QueryOperator(nameof(AnotherNestedResponseFromOperators.BoolVal), QueryOperators.Equal)]
            public bool BoolVal { get; set; }
        }

        private class ResponseFromOperators
        {
            public bool BoolVal { get; set; }
            public DateTime DateVal { get; set; }
            public int OneInt { get; set; }
            public int? OneNullableInt { get; set; }
            public decimal DecimalVal { get; set; }
            public string  StringVal { get; set; }
            public NestedResponseFromOperators NestedResponse { get; set; }
        }

        private sealed class NestedResponseFromOperators
        {
            public int OneInt { get; set; }
            public AnotherNestedResponseFromOperators AnotherNestedResponse { get; set; }
        }

        private sealed class AnotherNestedResponseFromOperators
        {
            public bool BoolVal { get; set; }
        }
    }
}
