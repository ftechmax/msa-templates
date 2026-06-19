# API project

The API service is responsible for exposing the domain over HTTP. It is intentionally thin: it serves queries from a local read model and forwards commands to the worker over the message bus. Business rules belong in the worker/domain layer; the API owns request validation, DTO mapping, reads, and client notifications.

## Responsibilities

The API should not contain domain logic. Its main jobs are:

- **Own the HTTP contract**: controllers, DTOs, a versioning strategy when you need one, and input validation.
- **Validate early**: reject bad requests before they become messages that bounce around the system.
- **Send commands** to the worker via the bus using MassTransit. The API should not need direct DB access to do writes.
- **Serve reads efficiently**: the template uses projections and caches them in Valkey/Redis so read paths stay fast without coupling reads to the write model.

The API also bridges worker events back to connected clients. It consumes published events from the worker and uses them to:

- **Invalidate/refresh caches** so later queries return updated view models.
- **Notify connected clients** through SignalR so UIs can update without polling.

## Operational notes

OpenTelemetry traces every request from controller through bus send, so an HTTP call and its eventual worker handling share a trace ID. The template also wires up `/health/live` and `/health/ready` for Kubernetes probes, and serves Swagger UI in `Development`.

## Async command loop

The API is the bridge between a synchronous web request and an asynchronous domain workflow.

From the browser's point of view, a command starts as a normal HTTP call. Inside the system, the API validates the request, maps it to a command, sends it to RabbitMQ, and returns immediately. The business outcome only exists after the worker has processed the command and published an event or fault.

This means the API has two distinct responsibilities in the same flow:

1. accept and validate the HTTP request
2. relay the eventual domain outcome back to the client side

In the template, those responsibilities are split across separate pieces:

- the controller and application service handle the HTTP-facing part
- the application service maps DTOs to commands and sends them onto the bus
- `LocalEventHandler` consumes the resulting events and faults
- that same handler invalidates caches and pushes updates to SignalR clients

After a POST or PUT, the response only tells the client that the API accepted and queued the command. The business result comes back later through the event or fault path.

The browser side of this flow is described in [Web status service and domain feedback](web.md#status-service-and-domain-feedback). The worker side is described in [Worker command handling and event publication](worker.md#command-handling-and-event-publication).

## Project Structure

The API's entry point is a set of HTTP endpoints (controllers). Each endpoint delegates to an application service. Queries are served from a local read model; writes turn into commands sent to the worker. The worker publishes events back, which the API consumes to invalidate caches and notify connected clients over SignalR.

### Controller

`ExampleController.cs` contains the HTTP endpoints for the `Example` domain. Add a controller per sub-domain instead of growing one controller indefinitely.

The controller does three things:

1. **Receive Request**: The controller accepts a request DTO from the client.
2. **Delegate to Application Service**: The controller forwards the call to the application service.
3. **Return Response**: For queries, the controller returns DTOs; for commands, it returns after the message is sent.

In this design, reads (GET endpoints) call local query methods, while writes (POST/PUT) forward commands to the worker.

### Application Service

The `ExampleService.cs` file contains the orchestration logic for the `Example` domain. It does not implement business rules; it coordinates reads and writes.

For queries, the application service:
1. **Check Cache**: Try to return a cached response.
2. **Load Read Model**: Load documents from the read model when the cache is empty.
3. **Return Result**: Map and return DTOs, then store the result in cache.

For commands, the application service:
1. **Receive Request**: Accept the request DTO from the controller.
2. **Create Command**: Convert the DTO into a domain command.
3. **Send Command**: Send the command to the worker over the bus.

This is intentionally thin. The API does not wait for the worker to finish domain processing before responding. That later outcome returns through the event path described below.

### Local Event Handler

The API subscribes to published events from the bus (e.g. `ExampleCreatedEvent`, `ExampleUpdatedEvent`). This is handled by `LocalEventHandler.cs`.

The local event handler:
1. **Receive Event**: The handler listens for events published by the worker.
2. **Invalidate Cache**: The handler removes affected cache entries so subsequent queries are consistent.
3. **Notify Clients**: The handler notifies connected clients (SignalR) that something changed.

This is the return path to the web app. The browser sends a command over HTTP, and the API forwards the resulting event or fault over SignalR.

#### Fault notifications

The handler also shows how to forward failures to clients by consuming `Fault<TCommand>` and publishing a `DomainFault` with a correlation id and trace id. Clients can use this information to correlate UI failures with logs and traces.

That fault path is especially important in a message-driven UI because the original HTTP request has already completed by the time the worker discovers the failure.

#### Using a Different Persistence Mechanism

Note that the default read model is served from the Valkey cache via `StackExchange.Redis`. To use another store, implement the repository interface for that backend.
