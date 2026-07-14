# Kubernetes manifests

The generated `k8s/` folder deploys the API, worker, web app, PostgreSQL database, and Valkey cache. The target cluster must already provide the shared platform services listed below.

These manifests assume [msa-infrastructure](https://github.com/ftechmax/msa-infrastructure/) or an equivalent platform. If you use something else, the capabilities listed below need to exist under the hostnames and CRDs the templates expect.

## What gets generated

The generator copies this `k8s/` structure into the new service project:

- `k8s/base/`
  - `api/`, `worker/`, and `web/` Deployments and Services
  - `cache/` resources for a per-service Valkey instance
  - `database/` resources for a per-service PostgreSQL StatefulSet
  - `rabbitmq-user.yaml` to provision a RabbitMQ user and permissions for the service
  - `api/httproute.yaml` and `web/httproute.yaml` for Istio Gateway API routing
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
- Istio 1.24+ or another Gateway API implementation, with both the standard **and experimental** channel CRDs from [kubernetes-sigs/gateway-api](https://github.com/kubernetes-sigs/gateway-api) installed. The experimental channel is required for the `CORS` filter.
- A `Gateway` resource that the generated `HTTPRoute`s can attach to. The generator defaults to a `Gateway` named `gateway` in namespace `istio-ingress`, matching Istio's Gateway API quick start. Override with the `Istio Gateway namespace` / `Istio Gateway name` prompts in the generator if your platform uses a different target.
- A container registry that your cluster can pull from

The generated base manifests do not install RabbitMQ, ingress, or the supporting operators. They only create the service-specific pieces that plug into that platform (the per-service Valkey cache and PostgreSQL database are included in the base manifests).

## Generator inputs and what they affect

The generator writes its prompt values into the manifests and application configuration.

- `Kubernetes namespace`
  - sets the namespace on the generated overlays
- `RabbitMQ host`
  - becomes the `rabbitmq__host` application setting for the API and worker
  - is also used to derive the RabbitMQ cluster namespace for the user/permission resources
- `Service name`
  - becomes the deployment names, service names, database name, and ingress hostname prefix
- `Istio Gateway namespace` / `Istio Gateway name`
  - become the `parentRefs` on the generated `HTTPRoute`s
- `Base domain`
  - is appended to the kebab-cased service name to form the `hostnames` entry on each `HTTPRoute`

## Routing model

The generated manifests assume path-based routing through Istio's Gateway API:

- `/` goes to the web app
- `/api/` goes to the API and is rewritten to `/`

Each `HTTPRoute` attaches to the shared `Gateway` you provided in the generator prompts, and is bound to a single literal hostname of the form `<service>.<domain>`. With the default prompts, that becomes `awesome-app.kube.local`.

Your ingress and DNS must expose hosts such as `awesome-app.your-domain.tld`. If they don't, edit the generated `HTTPRoute` resources or add a Kustomize patch for your environment.

## Overlay usage

### Local

Use `k8s/overlays/local` for a local or self-hosted cluster with a registry reachable as `registry:5000`.

It changes four parts of the base:

- adds example RabbitMQ and database secrets
- adds Pgweb at `applicationname-pgweb.<domain>` for local PostgreSQL inspection
- points images to `registry:5000/applicationname-*`
- relaxes some runtime settings that are handy in local development

If you use a different local registry, edit the image names before applying the overlay.

Pgweb uses the local database secret for both PostgreSQL access and HTTP basic auth. With the default generator prompts, it is available at `http://applicationname-pgweb.kube.local`.

### Development and production

`k8s/overlays/development` and `k8s/overlays/production` are template overlays. They still contain `{{REGISTRY}}` and `{{IMAGE_TAG}}` placeholders after generation.

Render those values in CI/CD or replace them manually before applying an overlay. The placeholders make the overlays invalid as direct deployment inputs.

## Applying the manifests

For a local-style setup:

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

The templates do not require that exact repo, but they do require an equivalent platform setup:

- RabbitMQ available by cluster DNS
- the CRDs/controllers referenced by the manifests, or your own replacements
- ingress routing via Gateway API `HTTPRoute` (or replace the generated routes with your own ingress resources)
- a pullable image registry

If your platform differs, treat the generated `k8s/` folder as a starting point and replace the parts that are platform-specific first: ingress mappings, user provisioning resources, image registry settings, and secret management.
