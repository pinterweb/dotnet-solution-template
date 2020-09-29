namespace BusinessApp.App.UnitTest
{
    using System.ComponentModel.DataAnnotations;

    public class CommandStub
    {}

    [Authorize]
    public class AuthCommandStub : CommandStub
    {}

    [Authorize("Foo", "Bar")]
    public class AuthRolesCommandStub : CommandStub
    {}

    public class DataAnnotatedCommandStub
    {
        [Compare(nameof(Bar))]
        public string CompareToBar { get; set; } = "lorem";

        [StringLength(10)]
        public string Foo { get; set; }

        [Required]
        public string Bar { get; set; } = "lorem";
    }

    public class QueryStub : IQuery<ResponseStub>
    {
        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is QueryStub other)
            {
                return other.Id == Id;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                return (HashingBase * HashingMultiplier) ^  Id.GetHashCode();
            }
        }
    }

    public class ResponseStub
    {}
}
