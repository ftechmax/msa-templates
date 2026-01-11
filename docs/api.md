# API project

The API service is responsible for exposing the domain over HTTP. It is intentionally thin: it serves queries from a local read model and forwards commands to the worker over the message bus. This keeps business logic out of the HTTP layer while still providing a clean API surface.

## Responsibilities

The API is intentionally not your domain engine. Its main jobs are:

- **Own the HTTP contract**: controllers, DTOs, a versioning strategy when you need one, and input validation.
- **Validate early**: reject bad requests before they become messages that bounce around the system.
- **Send commands** to the worker via the bus using MassTransit. The API should not need direct DB access to do writes.
- **Serve reads efficiently**: the template uses projections and caches them in Valkey/Redis so read paths stay fast without coupling reads to the write model.

On the “reactive” side, the API also acts as a bridge back to clients. It consumes domain events published by the worker and uses them to:

- **Invalidate/refresh caches** so clients see fresh view models.
- **Notify connected clients** through SignalR so UIs can update without polling.

## Operational notes

Like the worker, it’s wired for observability via OpenTelemetry and comes with the usual HTTP service niceties such as health checks and Swagger in development.

## Project Structure

The API project is very simple in how it works. Its entry point is a set of HTTP endpoints (controllers). Each endpoint delegates to an application service. Queries are served locally from a read model, while commands are sent to the worker. The worker publishes events, which the API consumes to invalidate caches and notify connected clients, creating a clean loop.

### Controller

The `ExampleController.cs` file contains the HTTP endpoints for the `Example` domain. If you have more sub-domains, you should create additional controllers following the same pattern instead of growing a single controller indefinitely.

The controller usually follows these steps:

1. **Receive Request**: The controller accepts a request DTO from the client.
2. **Delegate to Application Service**: The controller forwards the call to the application service.
3. **Return Response**: For queries, the controller returns DTOs; for commands, it returns after the message is sent.

In this design, reads (GET endpoints) call local query methods, while writes (POST/PUT) forward commands to the worker.

### Application Service

The `ExampleService.cs` file contains the orchestration logic for the `Example` domain. It does not implement business rules; it coordinates reads and writes.

For queries, the application service typically follows these steps:
1. **Check Cache**: Try to return a cached response.
2. **Load Read Model**: Load documents from the read model when the cache is empty.
3. **Return Result**: Map and return DTOs, then store the result in cache.

For commands, the application service typically follows these steps:
1. **Receive Request**: Accept the request DTO from the controller.
2. **Create Command**: Convert the DTO into a domain command.
3. **Send Command**: Send the command to the worker over the bus.

### Local Event Handler

The API subscribes to domain/integration events from the bus (e.g. `ExampleCreatedEvent`, `ExampleUpdatedEvent`). This is handled by `LocalEventHandler.cs`.

The local event handler typically follows these steps:
1. **Receive Event**: The handler listens for events published by the worker.
2. **Invalidate Cache**: The handler removes affected cache entries so subsequent queries are consistent.
3. **Notify Clients**: The handler notifies connected clients (SignalR) that something changed.

#### Fault notifications

The handler also shows how to forward failures to clients by consuming `Fault<TCommand>` and publishing a `DomainFault` with a correlation id and trace id. Clients can use this information to correlate UI failures with logs and traces.

#### Using a Different Persistence Mechanism

Note that the default read model is MongoDB. This will be replaced by a Valkey read model in the future. However, you can replace it with any other database by implementing the repository interface accordingly.