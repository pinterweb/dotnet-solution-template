using System;
using BusinessApp.Domain;

namespace BusinessApp.App
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class QueryOperatorAttribute : Attribute
    {
        /// <summary>
        /// Creates metadata for a property that has an operation operation to perform
        /// on a target property
        /// </summary>
        public QueryOperatorAttribute(string targetProp, string operatorToUse = QueryOperators.Equal)
        {
            OperatorToUse = operatorToUse ?? QueryOperators.Equal;
            TargetProp = targetProp.NotEmpty().Expect(nameof(targetProp));
        }

        /// <summary>
        /// Creates metadata for a property that has no operation, usually
        /// when the property is a nested class that has operations itself
        /// </summary>
        public QueryOperatorAttribute(string targetProp)
        {
            TargetProp = targetProp.NotEmpty().Expect(nameof(targetProp));
        }

        public string? OperatorToUse { get; }
        public string TargetProp { get; }
    }
}
