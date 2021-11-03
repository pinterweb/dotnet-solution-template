using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using BusinessApp.Test.Shared;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace BusinessApp.WebApi.IntegrationTest
{
    public class PostCommitRegistrationTests : IDisposable
    {
        private readonly Container container;
        private readonly Scope scope;
        private readonly IConfiguration config;
        private readonly IPostCommitHandler<RequestStub, ResponseStub> explicitInstance;

        public PostCommitRegistrationTests()
        {
            container = Startup.ConfigureContainer();
            scope = AsyncScopedLifestyle.BeginScope(container);
            container.CreateTestApp();

            explicitInstance = A.Fake<IPostCommitHandler<RequestStub, ResponseStub>>();
            container.Collection.AppendInstance(explicitInstance);
        }

        public void Dispose() => scope.Dispose();

        [Fact]
        public void NoneBatchRequest_CompositePostCommitRegisterUsed()
        {
            /* Arrange */
            container.Verify();
            var serviceType = typeof(IPostCommitHandler<RequestStub, ResponseStub>);

            /* Act */
            var _ = container.GetInstance(serviceType);

            /* Assert */
            var firstType = container.GetRegistration(serviceType);
            var producers = firstType
                .GetDependencies()
                .Prepend(firstType)
                .Select(ip => ip);

            /* Assert */
            Assert.Collection(producers,
                implType => Assert.Equal(
                    typeof(CompositePostCommitHandler<RequestStub, ResponseStub>),
                    implType.ImplementationType),
                implType => Assert.Equal(
                    typeof(IEnumerable<IPostCommitHandler<RequestStub, ResponseStub>>),
                    implType.ImplementationType),
                implType => Assert.Equal(
                    typeof(DummyPostCommitHandler),
                    implType.ImplementationType),
                implType => Assert.Same(
                    explicitInstance,
                    implType.GetInstance()));
        }

        [Fact]
        public void BatchRequest_BatchPostCommitRegisterUsed()
        {
            /* Arrange */
            container.Verify();
            var serviceType = typeof(IPostCommitHandler<IEnumerable<RequestStub>, IEnumerable<ResponseStub>>);

            /* Act */
            var _ = container.GetInstance(serviceType);

            /* Assert */
            var firstType = container.GetRegistration(serviceType);
            var producers = firstType
                .GetDependencies()
                .Prepend(firstType)
                .Select(ip => ip);

            Assert.Collection(producers,
                implType => Assert.Equal(
                    typeof(CompositePostCommitHandler<IEnumerable<RequestStub>, IEnumerable<ResponseStub>>),
                    implType.ImplementationType),
                implType => Assert.Equal(
                    typeof(IEnumerable<IPostCommitHandler<IEnumerable<RequestStub>, IEnumerable<ResponseStub>>>),
                    implType.ImplementationType),
                implType => Assert.Equal(
                    typeof(BatchPostCommitAdapter<RequestStub, ResponseStub>),
                    implType.ImplementationType));
        }

        private class DummyPostCommitHandler : IPostCommitHandler<RequestStub, ResponseStub>
        {
            public Task<Result<Unit, Exception>> HandleAsync(RequestStub request,
                ResponseStub response, CancellationToken cancelToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
