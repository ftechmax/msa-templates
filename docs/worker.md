# Worker project

The worker service is responsible for consuming commands and external events, applying business logic, persisting state, and publishing domain events. It typically runs as a background service and does not expose HTTP endpoints. The worker project template sets up a robust foundation for building message-driven services using MassTransit with RabbitMQ for messaging, MongoDB for persistence, and OpenTelemetry for observability.

## Responsibilities

In practice, the worker is where you put the “real work”:

- **Consumes commands** from the bus and treats them as the unit of work. Commands should be actionable and explicit, like `CreateOrder` instead of `OrderChanged`.
- **Handles external events** from other services and translates them into local commands when needed, so domain logic stays behind your own contracts.
- **Applies domain rules** and produces domain events as the outcome, meaning facts that happened, instead of leaking persistence concerns into handlers.
- **Persists state** (MongoDB by default) behind a repository abstraction.
- **Publishes events** back onto the bus so other services (and the API) can react asynchronously.

## Operational notes

- **Idempotent by design** because messages can be delivered more than once.
- **Observable**: traces, metrics, and logs are emitted via OpenTelemetry so you can answer “what happened to message X?”.
- **Resilient**: transient failures should be retryable and visible. MassTransit and RabbitMQ give you the building blocks, and you decide the policy.
- **Background jobs** belong here too. This template shows a hosted service for cache invalidation.

## Project Structure

The worker project is very simple in how it works. Because it is only allowed to communicate via messages, its entry point is a set of message handlers that respond to commands and events. Each handler delegates the actual business logic to a domain service, which encapsulates the core functionality of the service. The domain results in domain events, which are then published by the initial message handler, creating a clean loop.

### Command Handler

The `ExampleCommandHandler.cs` file contains the implementation of command handlers that process incoming commands. Each command handler is responsible for executing specific business logic when a command is received. This handler is designated to process `Example` domain commands. If you have more sub-domains, you should create additional command handlers following the same pattern instead of adding all command handlers to this single file.

The command handler usually follows these steps:
1. **Receive Command**: The handler listens for specific commands from the message bus.
2. **Pass Command to Domain Service**: The handler invokes the appropriate method on the domain service, passing along the entire command object.
3. **Convert Domain Events**: The domain service processes the command and returns a domain event object. The handler converts this domain event into an integration event suitable for publishing.
4. **Publish Integration Events**: After processing the command, the handler may publish domain events if the returned domain event object is not null.

### Domain Service

The `ExampleService.cs` file contains the core business logic for the `Example` domain. This service is responsible for processing commands delivered by its `CommandHandler` and generating domain events based on the business rules.

The domain service typically follows these steps:
1. **Receive Command**: The service receives the command object from the command handler.
2. **Create or Hydrate Aggregate**: The service either creates a new aggregate instance or retrieves an existing one from the repository, depending on the command.
3. **Apply Business Logic**: The service invokes methods on the aggregate to apply business rules and modify its state.
4. **Persist Changes**: The service saves the updated aggregate back to the repository.
5. **Return Domain Events**: The service returns any domain events that were generated as a result of processing the command.

#### Using a Different Persistence Mechanism

Note that the default persistence mechanism is MongoDB, but you can replace it with any other database by implementing the repository interface accordingly.
For event-sourcing scenarios, you can use my [MongoEventStore](https://github.com/ftechmax/mongo-eventstore) library as a starting point. In this case you would store the domain event object returned by the aggregate instead of persisting the aggregate state directly.

### External Event Handler

The `ExternalEventHandler.cs` file contains the implementation of event handlers that process incoming external events from other domains. Each event handler is responsible for executing specific business logic when an external event is received. This handler is designated to process external messages from the `Other.Worker.Contracts.Commands` namespace. An example would be handling a `UserCreatedEvent` from an identity service to create a corresponding local user profile. Or in our case we capture some remote code and link it to our `Example` aggregate.