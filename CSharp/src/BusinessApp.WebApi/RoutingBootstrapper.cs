﻿namespace BusinessApp.WebApi
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
#if winauth
    using Microsoft.AspNetCore.Authorization;
#endif
    using Microsoft.AspNetCore.Builder;
    using SimpleInjector;

//#if DEBUG
    using System.Collections.Generic;
    using System.Linq;
//#endif

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
        public EntityId(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public int ToInt32(IFormatProvider? provider) => Id;
        public TypeCode GetTypeCode() => Id.GetTypeCode();
        public static implicit operator int (EntityId id) => id.Id;
    }

    public class Get
    {
        public class Request : App.Query
        {
            public int Id { get; set; }
            public override IEnumerable<string> Sort { get; set; } = new List<string>();
        }

        public class Response
        {
            public Response(EntityId? id = null)
            {
                Id = id ?? new EntityId(0);
            }

            public EntityId Id { get; set; }
        }

        public class Handler :
            App.IRequestHandler<Request, IEnumerable<Response>>,
            App.IRequestHandler<Request, App.EnvelopeContract<Response>>
        {
            public Task<Result<IEnumerable<Response>, Exception>> HandleAsync(Request request, CancellationToken cancelToken)
            {
                var response =  new []
                {
                    new Response(new EntityId(1)),
                    new Response(new EntityId(2)),
                }
                .Where(r => r.Id.Id == request.Id);


                return Task.FromResult(Result.Ok(response));
            }

            Task<Result<App.EnvelopeContract<Response>, Exception>> App.IRequestHandler<Request, App.EnvelopeContract<Response>>.HandleAsync(
                Request request, CancellationToken cancelToken)
            {
                var e =  Result.Ok(new App.EnvelopeContract<Response>(
                    new[] { new Response() },
                    new App.Pagination
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

        public class Response : IEventStream
        {
            public IEnumerable<IDomainEvent> Events { get; set; } = new List<IDomainEvent>();

        }

        public class Event : IDomainEvent
        {
            public EntityId? Id { get; set; }

            public DateTimeOffset OccurredUtc { get; }
        }


        public class Handler : App.IRequestHandler<Query, Response>,
            IEventHandler<Event>
        {
            // to prevent infinite loops
            private readonly Event e = new Event();

            public Task<Result<Response, Exception>> HandleAsync(Query request, CancellationToken cancelToken)
            {
                var stream = new Response
                {
                    Events = new EventStream(new[] { new Event() })
                };

                return Task.FromResult(Result.Ok(stream));
            }

            public Task<Result<IEnumerable<IDomainEvent>, Exception>> HandleAsync(
                Event @event, CancellationToken cancelToken)
            {
                if (@event == e)
                {
                    return Task.FromResult(
                        Result.Ok<IEnumerable<IDomainEvent>>(new IDomainEvent[0]));
                }

                return Task.FromResult(
                    Result.Ok<IEnumerable<IDomainEvent>>(new[] { e }));
            }
        }
    }

    #endregion
//#endif
}
