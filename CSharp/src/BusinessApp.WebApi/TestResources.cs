using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;

namespace BusinessApp.WebApi
{
    [TypeConverter(typeof(EntityIdTypeConverter<EntityId, int>))]
    public class EntityId : IEntityId
    {
        public EntityId(int id) => Id = id;

        public int Id { get; set; }

        public int ToInt32(IFormatProvider? provider) => Id;
        public TypeCode GetTypeCode() => Id.GetTypeCode();
        public static implicit operator int(EntityId id) => id.Id;
    }

    public class Get
    {
        public class Request : Query
        {
            public int Id { get; set; }
            public override IEnumerable<string> Sort { get; set; } = new List<string>();
        }

        public class Response
        {
            public Response(EntityId? id = null) => Id = id ?? new EntityId(0);

            public EntityId Id { get; set; }
        }

        public class Handler :
            IRequestHandler<Request, IEnumerable<Response>>,
            IRequestHandler<Request, EnvelopeContract<Response>>
        {
            public Task<Result<IEnumerable<Response>, Exception>> HandleAsync(Request request, CancellationToken cancelToken)
            {
                var response = new[]
                {
                    new Response(new EntityId(1)),
                    new Response(new EntityId(2)),
                }
                .Where(r => r.Id.Id == request.Id);


                return Task.FromResult(Result.Ok(response));
            }

            Task<Result<EnvelopeContract<Response>, Exception>> IRequestHandler<Request, EnvelopeContract<Response>>.HandleAsync(
                Request request, CancellationToken cancelToken)
            {
                var e = Result.Ok(new EnvelopeContract<Response>(
                    new[] { new Response() },
                    new Pagination
                    {
                        ItemCount = 1
                    }));

                return Task.FromResult(e);
            }
        }
    }

    public class PostOrPut
    {
        public class Body
        {
            public long LongerId { get; set; }
            public EntityId? Id { get; set; }

            public override bool Equals(object unknown)
            {
                if (unknown is PostOrPut.Body other)
                {
                    return LongerId.Equals(other.LongerId);
                }

                return base.Equals(unknown);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + LongerId.GetHashCode();

                    return hash;
                }
            }
        }
    }

    public class Delete
    {
        public class Query
        {
            public EntityId? Id { get; set; }
        }

#if DEBUG
        public class Response : ICompositeEvent
        {
            public IEnumerable<IEvent> Events { get; set; } = new List<IEvent>();
        }

        public class WebDomainEvent : IEvent
        {
            public EntityId? Id { get; set; }

            public DateTimeOffset OccurredUtc { get; }
        }
#elif events
        public class Response : ICompositeEvent
        {
            public IEnumerable<IEvent> Events { get; set; } = new List<IEvent>();
        }

        public class WebDomainEvent : IEvent
        {
            public EntityId? Id { get; set; }

            public DateTimeOffset OccurredUtc { get; }
        }
#else
        public class Response
        { }
#endif
#if DEBUG
        public class Handler : Infrastructure.IRequestHandler<Query, Response>,
            IEventHandler<WebDomainEvent>
        {
            // to prevent infinite loops
            private readonly WebDomainEvent webEvent = new();

            public Task<Result<Response, Exception>> HandleAsync(Query request, CancellationToken cancelToken)
            {
                var events = new Response
                {
                    Events = new CompositeEvent(new[] { new WebDomainEvent() })
                };

                return Task.FromResult(Result.Ok(events));
            }

            public Task<Result<IEnumerable<IEvent>, Exception>> HandleAsync(
                WebDomainEvent e, CancellationToken cancelToken)
                => webEvent == e
                    ? Task.FromResult(
                        Result.Ok<IEnumerable<IEvent>>(Array.Empty<IEvent>()))
                    : Task.FromResult(Result.Ok<IEnumerable<IEvent>>(new[] { webEvent }));
        }
#elif events
        public class Handler : Infrastructure.IRequestHandler<Query, Response>,
            IEventHandler<WebDomainEvent>
        {
            // to prevent infinite loops
            private readonly WebDomainEvent webEvent = new();

            public Task<Result<Response, Exception>> HandleAsync(Query request, CancellationToken cancelToken)
            {
                var events = new Response
                {
                    Events = new CompositeEvent(new[] { new WebDomainEvent() })
                };

                return Task.FromResult(Result.Ok(events));
            }

            public Task<Result<IEnumerable<IEvent>, Exception>> HandleAsync(
                WebDomainEvent e, CancellationToken cancelToken)
                => webEvent == e
                    ? Task.FromResult(
                        Result.Ok<IEnumerable<IEvent>>(Array.Empty<IEvent>()))
                    : Task.FromResult(Result.Ok<IEnumerable<IEvent>>(new[] { webEvent }));
        }
#else
        public class Handler : Infrastructure.IRequestHandler<Query, Response>
        {
            public Task<Result<Response, Exception>> HandleAsync(Query request, CancellationToken cancelToken)
            {
                return Task.FromResult(Result.Ok(new Response()));
            }
        }
#endif
    }
}
