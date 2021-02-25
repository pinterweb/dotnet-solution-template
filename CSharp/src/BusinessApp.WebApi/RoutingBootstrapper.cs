namespace BusinessApp.WebApi
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

#if DEBUG
    using System.Collections.Generic;
    using System.Linq;
#endif

    /// <summary>
    /// Creates your routes
    /// </summary>
    public static class RoutingBootstrapper
    {
        public static void SetupEndpoints(this IApplicationBuilder app, Container container)
        {
            app.UseRouting();
#if cors
//#if DEBUG
            app.UseCors();
//#endif
#endif
#if winauth
            app.UseAuthentication();
            app.UseAuthorization();
#endif

            #region TODO APIS HERE

            app.UseEndpoints(endpoint =>
            {
                var endpoints = new IEndpointConventionBuilder[]
                {
#if DEBUG
                    endpoint.MapGet("/api/resources", async ctx =>
                        await container
                            .GetInstance<IHttpRequestHandler<Get.Request, IEnumerable<Get.Response>>>()
                            .HandleAsync(ctx, default)
                    ),
                    endpoint.MapGet("/api/resources/{id:int}", async ctx =>
                        await container
                            .GetInstance<IHttpRequestHandler<Get.Request, Get.Response>>()
                            .HandleAsync(ctx, default)
                    ),
                    endpoint.MapPost("/api/resources", async ctx =>
                        await container
                            .GetInstance<IHttpRequestHandler<PostOrPut.Body, PostOrPut.Body>>()
                            .HandleAsync(ctx, default)
                    ),
                    endpoint.MapPut("/api/resources/{id:int}", async ctx =>
                        await container
                            .GetInstance<IHttpRequestHandler<PostOrPut.Body, PostOrPut.Body>>()
                            .HandleAsync(ctx, default)
                    ),
                    endpoint.MapDelete("/api/resources/{id:int}", async ctx =>
                        await container
                            .GetInstance<IHttpRequestHandler<Delete.Query, Delete.Query>>()
                            .HandleAsync(ctx, default)
                    ),
#endif
                    //endpoint.MapGet("/api/<resource>", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<SomeQuery, IEnumerable<BusinessContract>>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapGet("/api/<resource>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<SomeQuery, BusinessContract>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapPost("/api/<resource>", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<BusinessCommand, BusinessCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapPut("/api/<resource>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<BusinessCommand, BusinessCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapDelete("/api/<resource>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<DeleteCommand, DeleteCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                };

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

#if DEBUG
    [System.ComponentModel.TypeConverter(typeof(EntityIdTypeConverter<EntityId, int>))]
    public class EntityId : IEntityId
    {
        public EntityId(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public int ToInt32(IFormatProvider provider) => Id;
        public TypeCode GetTypeCode() => Id.GetTypeCode();
    }

    public class Get
    {
        public class Request : App.Query
        {
            public int Id { get; set; }
            public override IEnumerable<string> Sort { get; set; }
        }

        public class Response
        {
            public EntityId Id { get; set; }
        }

        public class Handler :
            App.IRequestHandler<Request, IEnumerable<Response>>,
            App.IRequestHandler<Request, App.EnvelopeContract<Response>>
        {
            public Task<Result<IEnumerable<Response>, IFormattable>> HandleAsync(Request request, CancellationToken cancelToken)
            {
                var response =  new []
                {
                    new Response() { Id = new EntityId(1) },
                    new Response() { Id = new EntityId(2) },
                }
                .Where(r => r.Id.Id == request.Id);


                return Task.FromResult(Result<IEnumerable<Response>, IFormattable>.Ok(response));
            }

            Task<Result<App.EnvelopeContract<Response>, IFormattable>> App.IRequestHandler<Request, App.EnvelopeContract<Response>>.HandleAsync(
                Request request, CancellationToken cancelToken)
            {
                var e =  Result<App.EnvelopeContract<Response>, IFormattable>.Ok(new App.EnvelopeContract<Response>
                {
                    Data = new [] { new Response() },
                    Pagination = new App.Pagination
                    {
                        ItemCount = 1
                    }
                });

                return Task.FromResult(e);
            }
        }
    }

    public class PostOrPut
    {
        public class Body
        {
            public long LongerId { get; set; }
            public EntityId Id { get; set; }
        }

        public class Handler : App.IRequestHandler<Body, Body>
        {
            public Task<Result<Body, IFormattable>> HandleAsync(Body request, CancellationToken cancelToken)
            {
                return Task.FromResult(Result<Body, IFormattable>.Ok(request));
            }
        }
    }

    public class Delete
    {
        public class Query
        {
            public EntityId Id { get; set; }
        }

        public class Handler : App.IRequestHandler<Query, Query>
        {
            public Task<Result<Query, IFormattable>> HandleAsync(Query request, CancellationToken cancelToken)
            {
                return Task.FromResult(Result<Query, IFormattable>.Ok(request));
            }
        }
    }
#endif
}
