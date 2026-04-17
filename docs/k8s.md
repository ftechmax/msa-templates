# Kubernetes manifests

The generated `k8s/` folder is meant to deploy your API, worker, web app, and service-local cache into a cluster that already provides the shared platform services this stack depends on.

These manifests are designed to sit on top of [msa-infrastructure](https://github.com/ftechmax/msa-infrastructure/) or an equivalent platform. If you use something else, the important thing is that the capabilities below are present under the hostnames and CRDs the templates expect.

## What gets generated

The generator copies a ready-to-patch `k8s/` folder into your new service project.

- `k8s/base/`
  - `api/`, `worker/`, and `web/` Deployments and Services
  - `cache/` resources for a per-service Valkey instance
  - `rabbitmq-user.yaml` to provision a RabbitMQ user and permissions for the service
  - `database-user.yaml` to provision a FerretDB user for the service database
  - `api/mapping.yaml` and `web/mapping.yaml` for Emissary/Ambassador routing
- `k8s/overlays/local/`
  - points images at `registry:5000/...`, a locally deployed registry
  - includes example secrets for RabbitMQ and database credentials
  - removes some stricter deployment settings to make local iteration easier
- `k8s/overlays/development/` and `k8s/overlays/production/`
  - keep the base manifests intact
  - expect your delivery pipeline to replace `{{REGISTRY}}` and `{{IMAGE_TAG}}`

## Cluster prerequisites

Before applying the generated manifests, make sure your cluster already provides the following:

- A Kubernetes cluster with `kubectl` access
- A RabbitMQ deployment reachable from the generated namespace using the hostname you enter in the generator, for example `rabbitmq.rabbitmq-system.svc`
- The RabbitMQ CRDs/controllers needed for `rabbitmq.com/v1beta1` `User` and `Permission` resources, or an equivalent setup that lets those resources reconcile successfully
- A FerretDB deployment reachable using the hostname you enter in the generator, for example `ferretdb.ferretdb-system.svc`
- The custom `k8s.ftechmax.net/v1alpha1` `FerretDbUser` CRD/controller, or an equivalent mechanism if you plan to replace that part of the manifests
- Emissary/Ambassador ingress, or another ingress strategy after you replace the generated `getambassador.io/v3alpha1` `Mapping` resources
- A container registry that your cluster can pull from

The generated base manifests do not install RabbitMQ, FerretDB, ingress, or the supporting operators. They only create the service-specific pieces that plug into that platform.

## Generator inputs and what they affect

The interactive prompts are not cosmetic. They are stamped directly into the manifests and application configuration.

- `Kubernetes namespace`
  - sets the namespace on the generated overlays
- `RabbitMQ host`
  - becomes the `rabbitmq__host` application setting for the API and worker
  - is also used to derive the RabbitMQ cluster namespace for the user/permission resources
- `FerretDB host`
  - becomes part of the worker MongoDB connection string
- `Service name`
  - becomes the deployment names, service names, database name, and ingress host pattern

## Routing model

The generated manifests assume path-based routing through Emissary/Ambassador:

- `/` goes to the web app
- `/api/` goes to the API
- `/api-hub` goes to the SignalR hub

The generated mappings also expect requests to come in on a host matching `^applicationname\\..+$`, which becomes something like `^awesome-app\\..+$` after generation.

That means one of the following needs to be true:

- your ingress/DNS setup exposes hosts like `awesome-app.your-domain.tld`, or
- you edit the generated `Mapping` resources to match your environment

## Overlay usage

### Local

`k8s/overlays/local` is the easiest starting point when you have a local or self-hosted cluster and a registry reachable as `registry:5000`.

It does three things:

- adds example RabbitMQ and database secrets
- points images to `registry:5000/applicationname-*`
- relaxes some runtime settings that are handy in local development

If you use a different local registry, edit the image names before applying the overlay.

### Development and production

`k8s/overlays/development` and `k8s/overlays/production` are template overlays. They still contain `{{REGISTRY}}` and `{{IMAGE_TAG}}` placeholders after generation.

Those overlays are intended to be rendered by your CI/CD pipeline or replaced manually before you apply them. Out of the box, they are not directly deployable until those placeholders are filled in.

## Applying the manifests

For a local-style setup, the usual flow is:

1. Build and push `applicationname-worker`, `applicationname-api`, and `applicationname-web` images to the registry referenced by your overlay.
2. Adjust any placeholder secrets or hosts to match your cluster.
3. Apply the overlay:

```sh
kubectl apply -k k8s/overlays/local
```

For development or production overlays, make sure `{{REGISTRY}}` and `{{IMAGE_TAG}}` have been replaced before applying:

```sh
kubectl apply -k k8s/overlays/development
kubectl apply -k k8s/overlays/production
```

## If you are not using msa-infrastructure

That is completely fine. The templates do not require that exact repo, but they do require an equivalent platform shape:

- RabbitMQ available by cluster DNS
- FerretDB available by cluster DNS
- the CRDs/controllers referenced by the manifests, or your own replacements
- ingress routing for the generated host and path conventions
- a pullable image registry

If your platform differs, treat the generated `k8s/` folder as a starting point and replace the parts that are platform-specific first: ingress mappings, user provisioning resources, image registry settings, and secret management.
