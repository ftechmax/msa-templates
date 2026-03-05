#!/usr/bin/env bash
set -euo pipefail

GENERATOR_VERSION="0.0.0"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GITHUB_REPO="ftechmax/msa-templates"

if [ "$#" -lt 3 ]; then
  echo "Usage: $0 <destination_folder> <service_name> <rabbitmq_user_secret> [namespace]" >&2
  exit 1
fi

DESTINATION_FOLDER="$1"
SERVICE_NAME="$2"
RABBITMQ_USER_SECRET="$3"
NAMESPACE="${4:-default}"

# Validate service name is PascalCase
if [[ ! "$SERVICE_NAME" =~ ^[A-Z][a-zA-Z0-9]+$ ]]; then
  echo "Error: service_name must be PascalCase (e.g., AwesomeApp). Got: '$SERVICE_NAME'" >&2
  exit 1
fi

# Prepare service name
KEBAB_CASE_SERVICE_NAME=$(printf '%s' "$SERVICE_NAME" | sed -E 's/([A-Z]+)([A-Z][a-z])/\1-\2/g' | sed -E 's/([a-z0-9])([A-Z])/\1-\2/g' | tr '[:upper:]' '[:lower:]')
DOT_CASE_SERVICE_NAME=$(printf '%s' "$SERVICE_NAME" | sed -E 's/([A-Z])/.\1/g' | sed -E 's/^\.//')

# Install matching template version
CSPROJ="$SCRIPT_DIR/src/MSA.Templates.csproj"
if [ -f "$CSPROJ" ]; then
  echo "Local source detected, packing templates..."
  rm -f "$SCRIPT_DIR/artifacts"/MSA.Templates.*.nupkg
  dotnet pack "$CSPROJ" -o "$SCRIPT_DIR/artifacts" --nologo -v quiet
  NUPKG=$(find "$SCRIPT_DIR/artifacts" -name 'MSA.Templates.*.nupkg' -print -quit)
  echo "Installing $NUPKG"
  dotnet new install "$NUPKG" --force
else
  echo "Installing MSA.Templates v$GENERATOR_VERSION"
  dotnet new install "MSA.Templates@$GENERATOR_VERSION"
fi

# Resolve k8s source folder
if [ -d "$SCRIPT_DIR/k8s" ]; then
  K8S_SOURCE="$SCRIPT_DIR/k8s"
else
  echo "Downloading k8s manifests for v$GENERATOR_VERSION..."
  TEMP_DIR=$(mktemp -d)
  trap 'rm -rf "$TEMP_DIR"' EXIT
  DOWNLOAD_URL="https://github.com/$GITHUB_REPO/releases/download/$GENERATOR_VERSION/msa-generator-k8s-$GENERATOR_VERSION.zip"
  curl -fsSL "$DOWNLOAD_URL" -o "$TEMP_DIR/k8s.zip"
  unzip -q "$TEMP_DIR/k8s.zip" -d "$TEMP_DIR"
  K8S_SOURCE="$TEMP_DIR/k8s"
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
  sed -e "s/{{RABBITMQ-SECRET-NAME}}/$RABBITMQ_USER_SECRET/g" \
      -e "s/{{NAMESPACE}}/$NAMESPACE/g" \
      -e "s/applicationname/$KEBAB_CASE_SERVICE_NAME/g" \
      -e "s/ApplicationName/$DOT_CASE_SERVICE_NAME/g" \
      "$file_path" > "$tmp_file"
  mv "$tmp_file" "$file_path"
done

# Create krun.json
KRUN_JSON_PATH="$PROJECT_FOLDER/krun.json"
cat > "$KRUN_JSON_PATH" <<'EOF'
[
    {
        "name": "applicationname-worker",
        "path": "src/worker",
        "dockerfile": "ApplicationName.Worker",
        "context": "src/",
        "intercept_port": 5001
    },
    {
        "name": "applicationname-api",
        "path": "src/api",
        "dockerfile": "ApplicationName.Api",
        "context": "src/",
        "intercept_port": 5000
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
