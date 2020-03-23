namespace BusinessApp.App
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public static class ValidationResultExtensions
    {
        public static ValidationResult CreateWithIndexName(this ValidationResult result, int index)
        {
            if (result.MemberNames.Where(m => !string.IsNullOrWhiteSpace(m)).Any())
            {
                var indexMemberNames = result.MemberNames
                    .Select(n => n.CreateIndexName(index))
                    .ToList();

                return new ValidationResult(result.ErrorMessage, indexMemberNames);
            }

            return result;
        }
    }
}
