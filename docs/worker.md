# Worker project

The worker service is responsible for consuming commands and external events, applying business logic, persisting state, and publishing events that represent domain outcomes. It runs as a background service and does not expose HTTP endpoints. The template wires up MassTransit on RabbitMQ for messaging, Npgsql for persistence (PostgreSQL JSONB, an in-cluster Postgres StatefulSet in the default Kubernetes setup), and OpenTelemetry for traces, metrics, and logs.

## Responsibilities

In this stack, the worker owns the domain work:

- **Consumes commands** from the bus and treats them as the unit of work. Commands should be actionable and explicit, like `CreateOrder` instead of `OrderChanged`.
- **Handles external events** from other services and translates them into local commands when needed, keeping domain logic behind your own contracts.
- **Applies domain rules** and produces internal domain events as the outcome, rather than leaking persistence concerns into handlers.
- **Persists state** behind a repository abstraction, storing documents as PostgreSQL JSONB by default.
- **Publishes shared events** back onto the bus so other services (and the API) can react asynchronously.

## Operational notes

- Handlers must be **idempotent** because RabbitMQ can redeliver the same message after a worker restart or ack timeout.
- OpenTelemetry exports traces, metrics, and logs, so you can answer "what happened to message X?" from a single trace ID.
- Transient failures go through MassTransit's retry pipeline, and unrecoverable ones land in `_error` queues. The template ships defaults; you set the retry counts and backoff per consumer.
- Background jobs belong here too. The template shows a hosted service for cache invalidation.

## Command handling and event publication

The worker turns accepted commands into persisted state changes and published events.

That distinction matters across the whole stack:

- the web sends a command and gets an immediate HTTP response
- the API puts the command on the bus and returns
- the worker later consumes that command, applies domain logic, and publishes the resulting event

For the browser-side view of the same loop, see [Web status service and domain feedback](web.md#status-service-and-domain-feedback). For the HTTP and SignalR bridge around the worker, see [API async command loop](api.md#async-command-loop).

In these docs, "domain event" means the internal result produced by the application/domain layer. Once that result is mapped to a shared contract and published on the bus, it becomes a published event that the API and web app can react to.

In the template, the worker side is split into two layers:

- the command handler is the messaging boundary
- the application/domain service applies business rules and persists state

The command flow looks like this:

1. a command arrives from RabbitMQ
2. `ExampleCommandHandler` passes it to the application service
3. the application service loads or creates the aggregate and applies domain rules
4. the updated state is persisted
5. a domain event is returned from the application layer
6. the handler maps that domain event to a shared event contract
7. the handler republishes it onto the bus, preserving the original correlation id

That republished event is what the API later consumes to invalidate caches and notify the web app.

If command handling throws, the command has still been accepted into the asynchronous pipeline, but the business operation did not complete. That failure can travel back through the fault path so the API can surface a `DomainFault` to the browser.

## Project Structure

The worker only communicates via messages. Its entry point is a set of message handlers that respond to commands and events. Each handler delegates business logic to a domain service. The domain service returns internal domain events; the handler maps them to shared event contracts and publishes them back onto the bus.

### Command Handler

`ExampleCommandHandler.cs` handles incoming commands for the `Example` domain. Add separate command handlers for other sub-domains instead of putting every command in this file.

The command handler does four things:
1. **Receive Command**: The handler listens for specific commands from the message bus.
2. **Pass Command to Domain Service**: The handler invokes the appropriate method on the domain service, passing along the entire command object.
3. **Map Domain Events**: The domain service processes the command and returns a domain event object. The handler maps that domain event into a shared event contract suitable for publishing.
4. **Publish Events**: After processing the command, the handler may publish the mapped event if the returned domain event object is not null.

Notice what the handler does not do: it does not talk to the web directly and it does not complete an HTTP request. Its responsibility is to turn bus messages into durable domain outcomes and then publish the resulting facts back out.

### Domain Service

`ExampleService.cs` contains the business logic for the `Example` domain. It processes commands delivered by the command handler and returns internal domain events from the business rules it applies.

The domain service:
1. **Receive Command**: The service receives the command object from the command handler.
2. **Create or Hydrate Aggregate**: The service either creates a new aggregate instance or retrieves an existing one from the repository, depending on the command.
3. **Apply Business Logic**: The service invokes methods on the aggregate to apply business rules and modify its state.
4. **Persist Changes**: The service saves the updated aggregate back to the repository.
5. **Return Domain Events**: The service returns any domain events that were generated as a result of processing the command.

Returning the domain event instead of publishing it from inside the domain layer keeps the boundary explicit: domain code decides what happened; the outer consumer decides how that fact is published.

#### Using a Different Persistence Mechanism

Note that the default persistence mechanism stores documents as PostgreSQL JSONB via Npgsql; in the generated Kubernetes setup that means an in-cluster Postgres StatefulSet. Each document type maps to its own table (`id`/`created`/`updated` columns plus a `data jsonb` column), and the schema is created idempotently on startup. To use another database, implement the repository interface for that store.
For event-sourcing scenarios you would store the domain event object returned by the aggregate instead of persisting the aggregate state directly.

### External Event Handler

`ExternalEventHandler.cs` consumes external domain events. In a real service this could be something like handling `UserCreatedEvent` from an identity service and creating a local profile.
