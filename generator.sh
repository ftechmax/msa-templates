#!/usr/bin/env bash
set -euo pipefail

GENERATOR_VERSION="develop"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

prompt() {
  local var_name="$1" prompt_text="$2" default="$3"
  local input value
  while true; do
    if [ -n "$default" ]; then
      read -rp "$prompt_text [$default]: " input
    else
      read -rp "$prompt_text: " input
    fi
    value="${input:-$default}"
    if [ -z "$value" ]; then
      echo "  A value is required." >&2
      continue
    fi
    value="${value/#\~/$HOME}"
    break
  done
  eval "$var_name=\"\$value\""
}

prompt_pascal() {
  local var_name="$1" prompt_text="$2"
  local input
  while true; do
    read -rp "$prompt_text: " input
    if [ -z "$input" ]; then
      echo "  A value is required." >&2
      continue
    fi
    if [[ ! "$input" =~ ^[A-Z][a-zA-Z0-9]+$ ]]; then
      echo "  Must be PascalCase (e.g., AwesomeApp)." >&2
      continue
    fi
    break
  done
  eval "$var_name=\"\$input\""
}

prompt_host() {
  local var_name="$1" prompt_text="$2" default="$3"
  local input value
  while true; do
    read -rp "$prompt_text [$default]: " input
    value="${input:-$default}"
    if [[ ! "$value" =~ ^[a-zA-Z0-9-]+\.[a-zA-Z0-9-]+\.svc$ ]]; then
      echo "  Must match <name>.<namespace>.svc format." >&2
      continue
    fi
    break
  done
  eval "$var_name=\"\$value\""
}

# Interactive configuration
echo "MSA Generator $GENERATOR_VERSION"
echo "-------------------"

prompt        DESTINATION_FOLDER  "Destination folder" "~/git"
prompt_pascal SERVICE_NAME        "Service name (PascalCase)"
prompt        NAMESPACE           "Kubernetes namespace" "default"
prompt_host   RABBITMQ_HOST       "RabbitMQ host" "rabbitmq.rabbitmq-system.svc"
prompt_host   FERRETDB_HOST       "FerretDB host" "ferretdb.ferretdb-system.svc"

# Extract namespace from RabbitMQ host
RABBITMQ_CLUSTER_NAMESPACE=$(echo "$RABBITMQ_HOST" | cut -d'.' -f2)

# Prepare service name
KEBAB_CASE_SERVICE_NAME=$(printf '%s' "$SERVICE_NAME" | sed -E 's/([A-Z]+)([A-Z][a-z])/\1-\2/g' | sed -E 's/([a-z0-9])([A-Z])/\1-\2/g' | tr '[:upper:]' '[:lower:]')
DOT_CASE_SERVICE_NAME=$(printf '%s' "$SERVICE_NAME" | sed -E 's/([A-Z]+)([A-Z][a-z])/\1.\2/g' | sed -E 's/([a-z0-9])([A-Z])/\1.\2/g')
SNAKE_CASE_SERVICE_NAME=$(printf '%s' "$KEBAB_CASE_SERVICE_NAME" | tr '-' '_')

# Install matching template version
CSPROJ="$SCRIPT_DIR/src/MSA.Templates.csproj"
if [ -f "$CSPROJ" ]; then
  echo "Local source detected, packing templates..."
  rm -f "$SCRIPT_DIR/artifacts"/MSA.Templates.*.nupkg
  dotnet pack "$CSPROJ" -o "$SCRIPT_DIR/artifacts" --nologo -v quiet
  NUPKG=$(find "$SCRIPT_DIR/artifacts" -name 'MSA.Templates.*.nupkg' -print -quit)
  # Uninstall any existing version to avoid conflicts
  dotnet new uninstall MSA.Templates 2>/dev/null || true
  echo "Installing $NUPKG"
  dotnet new install "$NUPKG"
else
  echo "Installing MSA.Templates v$GENERATOR_VERSION"
  dotnet new uninstall MSA.Templates 2>/dev/null || true
  dotnet new install "MSA.Templates@$GENERATOR_VERSION"
fi

# Resolve k8s source folder
if [ -d "$SCRIPT_DIR/k8s" ]; then
  K8S_SOURCE="$SCRIPT_DIR/k8s"
else
  NUGET_PACKAGES="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
  K8S_SOURCE="$NUGET_PACKAGES/msa.templates/$GENERATOR_VERSION/k8s"
  TEMPLATE_NUPKG=""

  if [ ! -d "$K8S_SOURCE" ]; then
    TEMPLATE_HOME="${DOTNET_CLI_HOME:-$HOME}"
    TEMPLATE_NUPKG="$TEMPLATE_HOME/.templateengine/packages/MSA.Templates.$GENERATOR_VERSION.nupkg"
    if [ -f "$TEMPLATE_NUPKG" ]; then
      TEMP_K8S_DIR=$(mktemp -d)
      trap 'rm -rf "$TEMP_K8S_DIR"' EXIT
      unzip -q "$TEMPLATE_NUPKG" 'k8s/*' -d "$TEMP_K8S_DIR"
      K8S_SOURCE="$TEMP_K8S_DIR/k8s"
    fi
  fi

  if [ ! -d "$K8S_SOURCE" ]; then
    echo "k8s manifests not found in $K8S_SOURCE or $TEMPLATE_NUPKG" >&2
    exit 1
  fi
fi

# Prepare project folder
PROJECT_FOLDER="$DESTINATION_FOLDER/$KEBAB_CASE_SERVICE_NAME"
if [ ! -d "$PROJECT_FOLDER" ]; then
  mkdir -p "$PROJECT_FOLDER"
  echo "Created project folder: $PROJECT_FOLDER"
else
  echo "Project folder already exists: $PROJECT_FOLDER"
fi

# Generate projects
echo "Generating src/shared"
dotnet new msa-shared -n "$DOT_CASE_SERVICE_NAME" -o "$PROJECT_FOLDER/src/shared"

echo "Generating src/worker"
dotnet new msa-worker -n "$DOT_CASE_SERVICE_NAME" -o "$PROJECT_FOLDER/src/worker"

echo "Generating src/api"
dotnet new msa-api -n "$DOT_CASE_SERVICE_NAME" -o "$PROJECT_FOLDER/src/api"

echo "Generating src/web"
dotnet new msa-web -n "$DOT_CASE_SERVICE_NAME" -o "$PROJECT_FOLDER/src/web"

# Patch k8s folder
cp -R "$K8S_SOURCE" "$PROJECT_FOLDER"
find "$PROJECT_FOLDER/k8s" -type f -print0 | while IFS= read -r -d '' file_path; do
  echo "Patching $file_path"
  tmp_file="$file_path.tmp"
  sed -e "s/{{NAMESPACE}}/$NAMESPACE/g" \
      -e "s/{{RABBITMQ_HOST}}/$RABBITMQ_HOST/g" \
      -e "s/{{RABBITMQ_CLUSTER_NAMESPACE}}/$RABBITMQ_CLUSTER_NAMESPACE/g" \
      -e "s/{{FERRETDB_HOST}}/$FERRETDB_HOST/g" \
      -e "s/applicationname_snake/$SNAKE_CASE_SERVICE_NAME/g" \
      -e "s/applicationname/$KEBAB_CASE_SERVICE_NAME/g" \
      -e "s/ApplicationName/$DOT_CASE_SERVICE_NAME/g" \
      "$file_path" > "$tmp_file"
  mv "$tmp_file" "$file_path"
done

# Create krun.json
KRUN_JSON_PATH="$PROJECT_FOLDER/krun.json"
cat > "$KRUN_JSON_PATH" <<EOF
[
  {
    "name": "applicationname-worker",
    "path": "src/worker",
    "dockerfile": "ApplicationName.Worker",
    "context": "src/",
    "intercept_port": 5001,
    "service_dependencies": [
      { "host": "applicationname-cache", "port": 6379 },
      { "host": "$RABBITMQ_HOST", "port": 5672 },
      { "host": "$FERRETDB_HOST", "port": 27017 }
    ]
  },
  {
    "name": "applicationname-api",
    "path": "src/api",
    "dockerfile": "ApplicationName.Api",
    "context": "src/",
    "intercept_port": 5000,
    "service_dependencies": [
      { "host": "applicationname-cache", "port": 6379 },
      { "host": "$RABBITMQ_HOST", "port": 5672 }
    ]
  },
  {
    "name": "applicationname-web",
    "path": "src/web",
    "dockerfile": ".",
    "context": "src/web"
  }
]
EOF

sed -i \
  -e "s/applicationname/$KEBAB_CASE_SERVICE_NAME/g" \
  -e "s/ApplicationName/$DOT_CASE_SERVICE_NAME/g" \
  "$KRUN_JSON_PATH"

echo "Created krun.json: $KRUN_JSON_PATH"
