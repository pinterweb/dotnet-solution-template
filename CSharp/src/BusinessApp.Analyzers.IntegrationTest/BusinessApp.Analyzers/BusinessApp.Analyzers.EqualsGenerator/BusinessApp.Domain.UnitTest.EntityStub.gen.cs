#nullable enable
namespace BusinessApp.Domain.UnitTest
{
    public partial class EntityStub
    {
        public override bool Equals(object? obj)
        {
            if (obj is EntityStub other)
            {
                return string.Equals(Id, other.Id, global::System.StringComparison.OrdinalIgnoreCase);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = Id == null ? 0 : hash * 23 +  global::System.StringComparer.OrdinalIgnoreCase.GetHashCode(Id);

                return hash;
            }
        }
    }
}
#nullable restore