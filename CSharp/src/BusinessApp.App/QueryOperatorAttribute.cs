namespace BusinessApp.App
{
    using System;
    using BusinessApp.Domain;

    [AttributeUsage(AttributeTargets.Property)]
    public class QueryOperatorAttribute : Attribute
    {
        public QueryOperatorAttribute(string targetProp, string operatorToUse = QueryOperators.Equal)
        {
            OperatorToUse = operatorToUse ?? QueryOperators.Equal;
            TargetProp = GuardAgainst.Empty(targetProp, nameof(targetProp));
        }

        public string OperatorToUse { get; }
        public string TargetProp { get; }
    }
}
