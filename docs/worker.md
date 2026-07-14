# Worker project

The worker consumes commands and external events, applies business rules, persists state, and publishes the outcome. It runs as a background service without HTTP endpoints. Conveyo and RabbitMQ handle messaging, Npgsql stores JSONB documents in PostgreSQL, and OpenTelemetry records traces, metrics, and logs.

## Responsibilities

The worker owns the domain work:

- consumes explicit commands such as `CreateOrder` and treats them as the unit of work, not ambiguous messages such as `OrderChanged`
- translates external events into local commands when another service triggers domain work
- applies domain rules and returns internal domain events
- stores state behind a repository abstraction, using PostgreSQL JSONB by default
- publishes shared event contracts for the API and other services

## Operational notes

- Handlers must be idempotent because RabbitMQ can redeliver a message after a worker restart or acknowledgement timeout.
- OpenTelemetry exports traces, metrics, and logs, so you can answer "what happened to message X?" from a single trace ID.
- Transient failures go through Conveyo's retry pipeline with exponential backoff, and unrecoverable ones are published as a `Fault` and moved to a `{queue}_error` queue. The template ships defaults; you set the retry counts and backoff per consumer.
- Background jobs belong here too. The template shows a hosted service for cache invalidation.

## Command handling and event publication

The worker turns accepted commands into persisted state changes and published events.

The web receives an HTTP response as soon as the API queues a command. The worker processes that command later and publishes the result.

For the browser-side view of the same loop, see [Web asynchronous command results](web.md#asynchronous-command-results). For the HTTP and Server-Sent Events bridge around the worker, see [API async command loop](api.md#async-command-loop).

In these docs, a *domain event* is the internal result from the application/domain layer. The command handler maps it to a shared event contract before publishing it for the API and other services.

The worker splits that work across two layers:

- the command handler is the messaging boundary
- the application/domain service applies business rules and persists state

The command flow looks like this:

1. A command arrives from RabbitMQ.
2. `ExampleCommandHandler` passes it to the application service.
3. The service loads or creates the aggregate, then applies domain rules.
4. The service persists the updated state and returns a domain event.
5. The handler maps that event to a shared contract.
6. The handler publishes the contract with the original correlation ID.

If command handling throws, the command remains accepted by the asynchronous pipeline but the business operation has failed. Conveyo can publish a fault, which the API forwards to the browser as a `DomainFault`.

## Project structure

Message handlers are the worker's entry points. They delegate business logic to a domain service, map its internal domain events to shared contracts, and publish those contracts.

### Command handler

`ExampleCommandHandler.cs` handles incoming commands for the `Example` domain. Add separate command handlers for other sub-domains instead of putting every command in this file.

The handler listens for its command types and passes the complete command to the matching domain service method. If the service returns a domain event, the handler maps and publishes it as a shared event contract.

It never talks to the web or completes an HTTP request. Its boundary starts and ends on the message bus.

### Domain service

`ExampleService.cs` contains the business logic for the `Example` domain. It processes commands delivered by the command handler and returns internal domain events from the business rules it applies.

The service receives a command, creates or loads the aggregate, applies its business rules, and saves the new state. It returns any domain events produced by the aggregate.

The domain code decides what happened. The outer command handler decides how to publish it.

#### Replacing persistence

By default, Npgsql stores documents as PostgreSQL JSONB. The Kubernetes manifests run PostgreSQL as an in-cluster StatefulSet. Each document type has its own table with `id`, `created`, `updated`, and `data jsonb` columns. Startup creates the schema idempotently.

Implement the repository interface to use another database. For event sourcing, store the domain events returned by the aggregate instead of its current state.

### External event handler

`ExternalEventHandler.cs` consumes domain events from other services. For example, it could handle `UserCreatedEvent` from an identity service and create a local profile.
