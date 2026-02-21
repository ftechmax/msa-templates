#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -lt 3 ]; then
  echo "Usage: $0 <destination_folder> <service_name> <rabbitmq_user_secret>" >&2
  exit 1
fi

DESTINATION_FOLDER="$1"
SERVICE_NAME="$2"
RABBITMQ_USER_SECRET="$3"

# Prepare service name
KEBAB_CASE_SERVICE_NAME=$(printf '%s' "$SERVICE_NAME" | sed -E 's/([A-Z])/-\1/g' | sed -E 's/^-//' | sed -E 's/ /-/g' | tr '[:upper:]' '[:lower:]')

echo "Service name: $KEBAB_CASE_SERVICE_NAME"

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
dotnet new msa-shared -n "$SERVICE_NAME" -o "$PROJECT_FOLDER/src/shared"

echo "Generating src/worker"
dotnet new msa-worker -n "$SERVICE_NAME" -o "$PROJECT_FOLDER/src/worker"

echo "Generating src/api"
dotnet new msa-api -n "$SERVICE_NAME" -o "$PROJECT_FOLDER/src/api"

echo "Generating src/web"
dotnet new msa-web -n "$SERVICE_NAME" -o "$PROJECT_FOLDER/src/web"

# Patch k8s folder
cp -R "./k8s" "$PROJECT_FOLDER"
find "$PROJECT_FOLDER/k8s" -type f -print0 | while IFS= read -r -d '' file_path; do
  echo "Patching $file_path"
  tmp_file="$file_path.tmp"
  sed -e "s/{{RABBITMQ-SECRET-NAME}}/$RABBITMQ_USER_SECRET/g" \
      -e "s/applicationname/$KEBAB_CASE_SERVICE_NAME/g" \
      -e "s/ApplicationName/$SERVICE_NAME/g" \
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
  -e "s/ApplicationName/$SERVICE_NAME/g" \
  "$KRUN_JSON_PATH"

echo "Created krun.json: $KRUN_JSON_PATH"
