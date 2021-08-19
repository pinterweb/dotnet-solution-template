using System.Collections.Generic;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class CommandStub
    {}

#if DEBUG
    public class CompositeEventStub : ICompositeEvent
    {
        public IEnumerable<IEvent> Events { get; set; } = new List<IEvent>();
    }
#elif events
    public class CompositeEventStub : ICompositeEvent
    {
        public IEnumerable<IDomainEvent> Events { get; set; } = new List<IDomainEvent>();
    }
#endif

    [Authorize]
    public class AuthCommandStub : CommandStub
    {}

    [Authorize("Foo", "Bar")]
    public class AuthRolesCommandStub : CommandStub
    {}

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
}
