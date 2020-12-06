namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using BusinessApp.Domain;

    [Serializable]
    public class MemberValidationException : Exception, IFormattable
    {
        public MemberValidationException(string memberName, IEnumerable<string> errors)
            : base($"'{memberName}' failed validation. See errors for more details")
        {
            MemberName = memberName.NotEmpty().Expect(nameof(memberName));
            Errors = errors.NotEmpty().Expect(nameof(errors)).ToList();
            Data.Add(memberName, Errors);
        }

        public string MemberName { get; }
        public IReadOnlyCollection<string> Errors { get; }

        public override string ToString() => ToString("G", null);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            return $"{MemberName} is invalid: {string.Join(", ", Errors)}";
        }
    }
}
