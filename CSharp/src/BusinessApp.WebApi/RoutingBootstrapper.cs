using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
#if winauth
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Builder;
using SimpleInjector;

//#if DEBUG
using System.Collections.Generic;
using System.Linq;
//#endif

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Creates your routes
    /// </summary>
    public static class RoutingBootstrapper
    {
        public static void SetupEndpoints(this IApplicationBuilder app, Container container)
        {
            #region TODO: Your APIS here. Replace the test examples with your actual endpoints

            app.UseEndpoints(endpoint =>
            {
//#if DEBUG
                // make a func so we can get it in the http request scope
                // and use other scope services
                Func<IHttpRequestHandler> getHandler = () => container.GetInstance<IHttpRequestHandler>();

                var endpoints = new IEndpointConventionBuilder[]
                {
                    endpoint.MapGet("/api/resources",
                        getHandler().HandleAsync<Get.Request, IEnumerable<Get.Response>>),

                    endpoint.MapGet("/api/resources/{id:int}",
                        getHandler().HandleAsync<Get.Request, Get.Response>),

                    endpoint.MapPost("/api/resources",
                        getHandler().HandleAsync<PostOrPut.Body, PostOrPut.Body>),

                    endpoint.MapPut("/api/resources/{id:int}",
                        getHandler().HandleAsync<PostOrPut.Body, PostOrPut.Body>),

                    endpoint.MapDelete("/api/resources/{id:int}",
                        getHandler().HandleAsync<Delete.Query, Delete.Response>),
                };
//#endif

#if winauth
                foreach (var ep in endpoints)
                {
                    ep.RequireAuthorization(new AuthorizeAttribute());
                }
#endif
            });

            #endregion
        }
    }

//#if DEBUG
    #region TODO DELETE THESE. These support the test apis which should be replaces by your implementation

    [System.ComponentModel.TypeConverter(typeof(EntityIdTypeConverter<EntityId, int>))]
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
        public class Request : Infrastructure.Query
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
            Infrastructure.IRequestHandler<Request, IEnumerable<Response>>,
            Infrastructure.IRequestHandler<Request, Infrastructure.EnvelopeContract<Response>>
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

            Task<Result<Infrastructure.EnvelopeContract<Response>, Exception>> Infrastructure.IRequestHandler<Request, Infrastructure.EnvelopeContract<Response>>.HandleAsync(
                Request request, CancellationToken cancelToken)
            {
                var e = Result.Ok(new Infrastructure.EnvelopeContract<Response>(
                    new[] { new Response() },
                    new Infrastructure.Pagination
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

    #endregion
//#endif
}
