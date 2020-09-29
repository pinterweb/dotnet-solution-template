namespace BusinessApp.App
{
    public static class MemberValidationExceptionExtensions
    {
        public static MemberValidationException CreateWithIndexName(this MemberValidationException ex, int index)
        {
            var indexedMemberName = ex.MemberName.CreateIndexName(index);

            return new MemberValidationException(indexedMemberName, ex.Errors);
        }
    }
}
