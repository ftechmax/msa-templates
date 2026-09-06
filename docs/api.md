# API project

The API exposes the domain over HTTP. It serves queries from a local read model and sends write commands to the worker over the message bus. Business rules stay in the worker. The API owns validation, DTO mapping, reads, and browser notifications.

## Responsibilities

The API does not contain domain logic. It:

- defines the HTTP contract through controllers, DTOs, and input validation
- rejects invalid requests before they reach the message bus
- sends commands to the worker through Conveyo instead of writing to its database
- serves projections that the worker maintains in Valkey, and caches the DTOs it maps from them

Published worker events return through the API. A local event handler:

- invalidates affected cache entries
- notifies connected clients through Server-Sent Events (SSE)

## Operational notes

OpenTelemetry traces a request from the controller through the bus send. The HTTP call and the worker handling share a trace ID. The API serves Swagger UI in `Development` and exposes two probe endpoints: `/healthz` for liveness, which runs no dependency checks so an outage never restarts the pod, and `/readyz` for readiness, which runs every check tagged `ready`.

Kestrel starts after every hosted service, and Conveyo connects to RabbitMQ in one of them, so the API answers neither endpoint for the first several seconds. The Deployment covers that with a startup probe: Kubernetes disables the liveness and readiness probes until it passes, so a slow bus connection cannot restart the pod. Readiness then keeps the Service from routing traffic to a pod that is not listening yet.

## Async command loop

The API is the bridge between a synchronous web request and an asynchronous domain workflow.

The browser starts a command with a normal HTTP call. The API validates the request, maps it to a command, sends it to RabbitMQ, and returns without waiting for the worker. Later, `LocalEventHandler` consumes the resulting event or fault, invalidates any affected cache entries, and pushes the outcome to SSE clients.

A POST or PUT returns `202 Accepted`, which only confirms that the API accepted and queued the command. It is not the business result.

The browser side of this flow is described in [Web asynchronous command results](web.md#asynchronous-command-results). The worker side is described in [Worker command handling and event publication](worker.md#command-handling-and-event-publication).

## Project structure

Controllers delegate to application services. Queries read the local model; writes become commands for the worker. `LocalEventHandler` handles the return path from worker events to the cache and connected browsers.

### Controller

`ExampleController.cs` contains the HTTP endpoints for the `Example` domain. Add a controller per sub-domain instead of growing one controller indefinitely.

The controller accepts a request DTO and delegates to the application service. GET endpoints return query DTOs. POST and PUT endpoints return `202 Accepted` once the service has sent the command to the bus.

### Request validation

`Validators` holds one `AbstractValidator<TDto>` per request DTO. `AddValidatorsFromAssemblyContaining<CreateExampleDtoValidator>()` registers them all, so every `IValidator<TDto>` resolves from the container.

An endpoint injects the validator it needs through the constructor and invokes it:

1. `await validator.ValidateAsync(dto)`.
2. on failure, `AddToModelState` copies the failures into model state and the action returns `ValidationProblem(ModelState)`.
3. on success the action calls the application service and continues on.

`ValidationResultExtensions.AddToModelState` registers each failure by its property path, so a nested rule is defined as `ExampleValueObject.Code`.

### Application service

`ExampleService.cs` coordinates reads and writes for the `Example` domain. It does not implement business rules.

For queries, the application service:

1. checks the DTO cache
2. loads projections from the read model after a cache miss
3. maps the projections to DTOs and caches the result

For commands, the application service:

1. accepts the request DTO
2. maps it to a domain command
3. sends the command over the bus

### Local event handler

`LocalEventHandler.cs` subscribes to events such as `ExampleCreatedEvent` and `ExampleUpdatedEvent`. It removes stale cache entries and broadcasts the event through `IServerSentEventsService`. Browsers connect to `/events`, exposed as `/api/events` through ingress, with the native `EventSource` API.

#### Fault notifications

The handler consumes `Fault<TCommand>` and sends a `DomainFault` containing the correlation ID and trace ID. Clients use those IDs to match a UI failure with its command, logs, and trace.

The original HTTP request has already completed when the worker reports this kind of failure.

#### Replacing the read store

The default read model uses Valkey through `StackExchange.Redis`. Implement the repository interface to use another store.
