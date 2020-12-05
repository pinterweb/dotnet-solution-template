namespace BusinessApp.App.UnitTest
{
    using System.Collections.Generic;
#if dataannotations
    using System.ComponentModel.DataAnnotations;
#endif

    public class CommandStub
    {}

    [Authorize]
    public class AuthCommandStub : CommandStub
    {}

    [Authorize("Foo", "Bar")]
    public class AuthRolesCommandStub : CommandStub
    {}

#if dataannotations
    public class DataAnnotatedCommandStub
    {
        [Compare(nameof(Bar))]
        public string CompareToBar { get; set; } = "lorem";

        [StringLength(10)]
        public string Foo { get; set; }

        [Required]
        public string Bar { get; set; } = "lorem";
    }
#endif

    public class QueryStub : IQuery
    {
        public int Id { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public IEnumerable<string> Sort { get; set; }
        public IEnumerable<string> Embed { get; set; }
        public IEnumerable<string> Expand { get; set; }
        public IEnumerable<string> Fields { get; set; }

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
