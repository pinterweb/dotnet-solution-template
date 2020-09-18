﻿namespace BusinessApp.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for the Unit of work pattern
    /// </summary>
    public interface IUnitOfWork
    {
        event EventHandler Committing;
        event EventHandler Committed;
        void Add(AggregateRoot aggregate);
        void Remove(AggregateRoot aggregate);
        void Track(AggregateRoot aggregate);
        Task CommitAsync(CancellationToken cancellationToken);
        Task RevertAsync(CancellationToken cancellationToken);
    }
}
