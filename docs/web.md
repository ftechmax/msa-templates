# Web project

The web template is a lightweight Angular SPA wired to the rest of the stack: HTTP calls go to the API, real-time updates come in through SignalR, and the production build is served by Nginx.

The template is intentionally small: it gives you a working shell that matches the API and worker templates without forcing a frontend architecture on day one.

## What is included

The generated web project comes with:

- Angular using standalone configuration
- Angular Material for the base shell and navigation
- Transloco for localization
- SignalR client wiring for server-pushed updates
- a shared status service (`EventService`) that fans those updates out to the UI
- a small example feature to replace with your own domain
- an Nginx-based production container image

## Project structure

The template is organized around a few simple integration points:

- `src/app/app.routes.ts`
  - root routing and lazy loading for the example feature
- `src/app/layout/`
  - the application shell and navigation
- `src/app/example/`
  - example contracts, routes, and HTTP client code you are expected to replace
- `src/app/signalr.service.ts`
  - creates the SignalR connection and listens for events from the API
- `src/app/status.service.ts`
  - acts as a small in-memory event bus for published events and domain faults inside the SPA
- `public/i18n/`
  - translation files loaded by Transloco

## HTTP integration

The template expects the API to be exposed under `/api`.

The generated example client follows that convention directly:

- `GET /api/example/`
- `GET /api/example/{id}`
- `POST /api/example/`
- `PUT /api/example/{id}`

That matches the generated ingress setup, where the web app is served from `/` and API traffic is routed separately to `/api/`.

## SignalR integration

Real-time updates are wired through `/api-hub`.

The generated `SignalRService`:

- creates a connection to `/api-hub`
- subscribes to example events such as `ExampleCreatedEvent`
- forwards those events into `EventService`
- exposes fault notifications so the UI can correlate failures with a command

The app starts that connection once at the root, so the event stream is available to whichever screens are active.

When you replace the example domain, rename the event handlers and group the subscriptions by feature or domain.

## Status service and domain feedback

`src/app/status.service.ts` is the UI-side entry point for asynchronous command results.

In code it is named `EventService`. It exposes published events and domain faults to the rest of the SPA without every component subscribing to SignalR directly.

For the server-side halves of the same loop, see [API async command loop](api.md#async-command-loop) and [Worker command handling and event publication](worker.md#command-handling-and-event-publication).

When the web app sends a command:

1. the browser calls the API over HTTP
2. the API validates the request and puts a command on RabbitMQ
3. the HTTP request returns quickly
4. the worker processes the command later
5. the worker publishes a resulting event that represents the domain outcome, or a fault if processing failed
6. the API forwards that outcome to the browser over SignalR
7. `SignalRService` pushes it into `EventService`
8. interested components react

At step 3 the API has accepted and queued the command. The domain result arrives later through step 6.

Without the status service, each feature would have to subscribe to SignalR directly and duplicate its own event/fault correlation logic.

```mermaid
sequenceDiagram
    participant UI as Web UI
    participant ES as SignalRService / EventService
    participant API as API
    participant MQ as RabbitMQ
    participant Worker as Worker

    UI->>API: POST /api/... (command + correlationId)
    API->>MQ: Send command
    API-->>UI: HTTP response returns immediately
    MQ->>Worker: Deliver command
    Worker->>Worker: Apply domain logic and persist state

    alt Success
        Worker->>MQ: Publish domain event
        MQ->>API: Deliver event
        API->>API: Invalidate caches and notify SignalR clients
        API-->>ES: Event over /api-hub
        ES-->>UI: Fan out event to subscribers
    else Failure
        Worker-->>MQ: Publish fault message
        MQ->>API: Deliver fault
        API-->>ES: DomainFault over /api-hub
        ES-->>UI: Fan out fault to subscribers
    end
```

### Separation of responsibilities

The status service keeps the responsibilities separated:

- the HTTP client sends commands and performs queries
- `SignalRService` deals with the transport and hub message names
- `EventService` exposes a shared stream of domain outcomes
- components subscribe only to the events they care about

This keeps the UI reactive without coupling every feature directly to SignalR internals.

It also lets multiple parts of the UI react to the same published event independently. When `ExampleCreatedEvent` arrives, a create form can navigate, a collection screen can refresh its list, and a notification area can show a toast. Three subscribers, one event.

### Example flow in the template

The create example uses this flow:

1. the form generates a `correlationId`
2. that `correlationId` is sent with the command payload
3. the HTTP call returns immediately after the API accepts the command
4. the component stays subscribed to `ExampleCreatedEvent` and `CreateExampleFault`
5. when an event comes back with the same `correlationId`, the UI knows it is the outcome of that specific user action

That lets the screen respond to real domain outcomes instead of pretending the POST response was the final truth.

In the template:

- the create component listens for `ExampleCreatedEvent` and navigates when the matching event arrives
- the create component also listens for `CreateExampleFault` and shows a snackbar when processing fails
- the collection component merges `ExampleCreatedEvent` into its reload stream so lists refresh when a create command eventually succeeds

The web template keeps those two moments separate: the user action can return immediately, and the UI can still wait for the real domain outcome.

### Correlation IDs

Correlation IDs make this practical.

Because many users and many commands may be in flight at once, the browser needs a way to match "this event" back to "that button click". The template does that by sending a correlation ID with the command and expecting the eventual event or fault to carry it back.

The status service carries the correlation ID through `SignalRService` into `EventService`, so the browser can tell which arriving event answers which earlier POST.

### How to extend it

As you replace the example domain:

1. add event and fault types to your contracts
2. add matching `Subject`s to `EventService`
3. register the corresponding SignalR handlers in `SignalRService`
4. subscribe from the components, stores, or feature services that care about those outcomes

Try to keep the responsibilities split the same way:

- `SignalRService` should translate transport messages into app events
- `EventService` should be the shared fan-out point
- feature code should react to domain outcomes, not parse hub wiring

With that boundary, `SignalRService` contains the hub method names, `EventService` contains the shared app events, and feature code only deals with domain outcomes.

## Translations

Translations are handled through Transloco.

- available languages are configured in `src/app/app.config.ts`
- translation files live in `public/i18n/`
- the template starts with English only

If you add more languages, update both the translation files and the Transloco configuration.

## Running locally

From the generated `src/web` folder:

```sh
npm ci
npx ng serve --proxy-config proxy.config.json
```

The proxy forwards `/api` and `/api-hub` to the generated service host so you can develop the frontend without changing the app code.

If your API is not reachable at the default host in `proxy.config.json`, update that file to match your environment before starting the dev server.

To create a production build:

```sh
npm run build
```

## Production container

The template includes a multi-stage Docker build:

1. build the Angular app with Node
2. serve the compiled assets from Nginx

The default Nginx config uses SPA-style fallback routing so deep links resolve to `index.html` instead of returning 404s.

## Replacing the example domain

The first cleanup pass after generation:

1. replace the example contracts in `src/app/example/contracts.ts`
2. replace the example HTTP client in `src/app/example/httpclient.ts`
3. replace the example routes and components with your real feature flow
4. rename the SignalR event listeners in `src/app/signalr.service.ts`
5. adjust the subjects in `src/app/status.service.ts`

Once those pieces are updated, the generated app is a normal Angular app with the message-driven wiring already in place.
