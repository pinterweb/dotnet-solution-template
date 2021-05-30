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
        }
    }

    public class Delete
    {
        public class Query
        {
            public EntityId? Id { get; set; }
        }

        public class Response : ICompositeEvent
        {
            public IEnumerable<IDomainEvent> Events { get; set; } = new List<IDomainEvent>();

        }

        public class WebDomainEvent : IDomainEvent
        {
            public EntityId? Id { get; set; }

            public DateTimeOffset OccurredUtc { get; }
        }


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

            public Task<Result<IEnumerable<IDomainEvent>, Exception>> HandleAsync(
                WebDomainEvent e, CancellationToken cancelToken)
                => webEvent == e
                    ? Task.FromResult(
                        Result.Ok<IEnumerable<IDomainEvent>>(Array.Empty<IDomainEvent>()))
                    : Task.FromResult(Result.Ok<IEnumerable<IDomainEvent>>(new[] { webEvent }));
        }
    }
}
