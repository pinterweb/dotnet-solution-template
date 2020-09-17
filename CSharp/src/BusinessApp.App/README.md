# Getting Started
    1. Create some Query objects that inherit from Query and IQuery<TRequest>
        - This will lead you to creating apis in the RoutingBootstrapper
    2. Create command, command handlers
        - This will lead you to creating aggregate roots and entities
    3. Create your data DTOs
        - If using EF, use IEntityConfiguration
        - If using EF, run migrations
    4. Register any external services that are not hadnlers with simpleinjector

# Security
    ## Role Authorization
        * Files involved:
            - AuthorizationCommandDecorator.cs
            - AuthorizationQueryDecorator.cs
            - AuthorizeAttribute.cs
            - AuthorizeAttributeHandler.cs
            - SecurityResourceException.cs
        Add `[Authorize]` to any command object to run role authorization

# Logging
    * Files involved:
        - BackgroundLogDecorator.cs
        - CompositeLogger.cs
        - ConsoleLogger.cs
        - FileLogger.cs
        - IFileProperties.cs
        - RollingFileProperties.cs
        - SerializedLogEntryFormatter.cs
        - StringLogEntryFormatter.cs
        - TraceLogger.cs
    * Default to file appender

# Batch Handling
    * Files involved:
        - BatchCommandGroupDecorator.cs
        - BatchCommandHandler.cs
        - IBatchGrouper.cs
        - NullBatchGrouper.cs
    * Default to file appender

# Validation
    * Files involved:
        - CompoisteValidationResult.cs
        - CompositeValidator.cs
        - DataAnnotationValidator.cs
        - FluentValidtaionValidator.cs
        - IValidator.cs
        - ValidateObjectAttribute.cs
        - ValidationBatchCommandDecorator.cs
        - ValidationCommandDecorator.cs
        - ValidationException.cs
        - ValidationResultExtensions.cs
    * Default to file appender

# Querying
    * Files involved:
        - EnvelopeContract.cs
        - QueryOperatorAttribute.cs
        - IQueryHandler.cs
        - Operator.cs
        - Pagination.cs
        - Query.cs
    * Default to file appender

# Command Handling
    * Files involved:
        - ICommandHandler.cs
        - ITransactionFactory.cs
        - TransactionDecorator.cs
    * Default to file appender

# Other cross cutting
    * Files involved:
        - BackgroundWorker.cs
        - CommunicationException.cs
        - DeadlockRetryDecorator.cs
        - EntityNotFoundQueryDecorator.cs
        - ISeralizer.cs
        - StringExtensions.cs
    * Default to file appender
