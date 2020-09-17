namespace BusinessApp.Data.IntegrationTest
{
    using System;
    using System.Collections.Generic;
    using BusinessApp.App;
    using BusinessApp.Data;
    using Xunit;

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

        public static IEnumerable<object[]> Integers  => new[]
        {
            new object[] { 1, 1 },
            new object[] { 1, 2 },
            new object[] { 1, 0 }
        };

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
            response.OneDate = b;
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
            response.OneDate = b;
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
            response.OneDate = b;
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
            response.ManyStrings = b;
            var spec = sut.Build(query);

            /* Act */
            var result = spec.IsSatisfiedBy(response);

            /* Assert */
            Assert.Equal(expect, result);
        }

        private sealed class QueryWithOperators
        {
            [QueryOperator(nameof(ResponseFromOperators.OneDate), QueryOperators.Equal)]
            public DateTime? OneDate_Eq { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.OneDate), QueryOperators.LessThanOrEqualTo)]
            public DateTime? OneDate_Lte { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.OneDate), QueryOperators.GreaterThanOrEqualTo)]
            public DateTime? OneDate_Gte { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.OneInt), QueryOperators.LessThan)]
            public int? OneInt_Lt { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.OneInt), QueryOperators.GreaterThan)]
            public int? OneInt_Gt { get; set; }

            [QueryOperator(nameof(ResponseFromOperators.ManyStrings), QueryOperators.Contains)]
            public IEnumerable<string> ManyString_Contains { get; set; }
        }

        private sealed class ResponseFromOperators
        {
            public DateTime OneDate { get; set; }
            public int OneInt { get; set; }
            public string  ManyStrings { get; set; }
        }
    }
}
