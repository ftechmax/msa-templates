#!/usr/bin/env bash
set -euo pipefail

GENERATOR_VERSION="develop"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RELEASE_BASE_URL="https://github.com/ftechmax/msa-templates/releases/download"

prompt_required() {
  local var_name="$1" prompt_text="$2" default="$3"
  local input value rc
  while true; do
    if [ -n "$default" ]; then
      read -rp "$prompt_text [$default]: " input && rc=0 || rc=$?
    else
      read -rp "$prompt_text: " input && rc=0 || rc=$?
    fi
    value="${input:-$default}"
    if [ -z "$value" ]; then
      if [ "$rc" -ne 0 ]; then
        echo "  Unexpected end of input." >&2
        exit 1
      fi
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
  local input rc
  while true; do
    read -rp "$prompt_text: " input && rc=0 || rc=$?
    if [ -z "$input" ]; then
      if [ "$rc" -ne 0 ]; then
        echo "  Unexpected end of input." >&2
        exit 1
      fi
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
    read -rp "$prompt_text [$default]: " input || input=""
    value="${input:-$default}"
    if [[ ! "$value" =~ ^[a-zA-Z0-9-]+\.[a-zA-Z0-9-]+\.svc$ ]]; then
      echo "  Must match <name>.<namespace>.svc format." >&2
      continue
    fi
    break
  done
  eval "$var_name=\"\$value\""
}

echo "MSA Generator $GENERATOR_VERSION"
echo "-------------------"

prompt_required DESTINATION_FOLDER  "Destination folder" "~/git"
prompt_pascal   SERVICE_NAME        "Service name (PascalCase)"
prompt_required NAMESPACE           "Kubernetes namespace" "default"
prompt_host     RABBITMQ_HOST       "RabbitMQ host" "rabbitmq.rabbitmq-system.svc"
prompt_required GATEWAY_NAMESPACE   "Istio Gateway namespace" "istio-ingress"
prompt_required GATEWAY_NAME        "Istio Gateway name" "gateway"
prompt_required DOMAIN              "Base domain" "kube.local"

# Extract namespace from RabbitMQ host (rabbitmq.rabbitmq-system.svc -> rabbitmq-system)
RABBITMQ_CLUSTER_NAMESPACE=$(echo "$RABBITMQ_HOST" | cut -d'.' -f2)

KEBAB_CASE_SERVICE_NAME=$(printf '%s' "$SERVICE_NAME" | sed -E 's/([A-Z]+)([A-Z][a-z])/\1-\2/g' | sed -E 's/([a-z0-9])([A-Z])/\1-\2/g' | tr '[:upper:]' '[:lower:]')
DOT_CASE_SERVICE_NAME=$(printf '%s' "$SERVICE_NAME" | sed -E 's/([A-Z]+)([A-Z][a-z])/\1.\2/g' | sed -E 's/([a-z0-9])([A-Z])/\1.\2/g')
SNAKE_CASE_SERVICE_NAME=$(printf '%s' "$KEBAB_CASE_SERVICE_NAME" | tr '-' '_')

# Resolve TEMPLATES_SOURCE and K8S_SOURCE
# 1. Sibling checkout / extracted bundle: templates/ + k8s/ next to script.
# 2. Auto-fetch: download msa-templates-v$GENERATOR_VERSION.zip from the
#    matching release and extract to a tempdir.

FETCH_TMPDIR=""
cleanup_fetch_tmpdir() {
  if [ -n "$FETCH_TMPDIR" ] && [ -d "$FETCH_TMPDIR" ]; then
    rm -rf "$FETCH_TMPDIR"
  fi
}
trap cleanup_fetch_tmpdir EXIT

if [ -d "$SCRIPT_DIR/templates" ] && [ -d "$SCRIPT_DIR/k8s" ]; then
  TEMPLATES_SOURCE="$SCRIPT_DIR/templates"
  K8S_SOURCE="$SCRIPT_DIR/k8s"
  echo "Using local templates: $TEMPLATES_SOURCE"
else
  if [ "$GENERATOR_VERSION" = "develop" ]; then
    echo "No sibling templates/ or k8s/ found and GENERATOR_VERSION is 'develop' (no matching release to fetch)." >&2
    echo "Run from a checkout or an extracted release bundle." >&2
    exit 1
  fi
  ZIP_NAME="msa-templates-v$GENERATOR_VERSION.zip"
  ZIP_URL="$RELEASE_BASE_URL/v$GENERATOR_VERSION/$ZIP_NAME"
  FETCH_TMPDIR=$(mktemp -d)
  echo "Downloading $ZIP_URL"
  curl -fsSL -o "$FETCH_TMPDIR/$ZIP_NAME" "$ZIP_URL"
  unzip -q "$FETCH_TMPDIR/$ZIP_NAME" -d "$FETCH_TMPDIR"
  BUNDLE_ROOT=$(find "$FETCH_TMPDIR" -maxdepth 2 -type d -name templates -print -quit)
  if [ -z "$BUNDLE_ROOT" ]; then
    echo "Could not locate templates/ inside $ZIP_NAME" >&2
    exit 1
  fi
  BUNDLE_ROOT=$(dirname "$BUNDLE_ROOT")
  TEMPLATES_SOURCE="$BUNDLE_ROOT/templates"
  K8S_SOURCE="$BUNDLE_ROOT/k8s"
  if [ ! -d "$K8S_SOURCE" ]; then
    echo "k8s/ not found inside $ZIP_NAME" >&2
    exit 1
  fi
fi

PROJECT_FOLDER="$DESTINATION_FOLDER/$KEBAB_CASE_SERVICE_NAME"
if [ ! -d "$PROJECT_FOLDER" ]; then
  mkdir -p "$PROJECT_FOLDER"
  echo "Created project folder: $PROJECT_FOLDER"
else
  echo "Project folder already exists: $PROJECT_FOLDER"
fi

# Content-substitution allowlist; keep in sync between sh and ps1
TEXT_EXTENSIONS=(.cs .csproj .sln .json .yaml .yml .ts .html .scss .css .md .conf .template .projitems .shproj .DotSettings)
TEXT_NAMES=(Dockerfile .dockerignore .editorconfig .gitignore)
TEXT_SKIP_NAMES=(package-lock.json)
EXCLUDED_DIRS=(bin obj dist node_modules .angular .vs .vscode)

is_text_target() {
  local name="$1" item ext
  for item in "${TEXT_SKIP_NAMES[@]}"; do
    [ "$name" = "$item" ] && return 1
  done
  for item in "${TEXT_NAMES[@]}"; do
    [ "$name" = "$item" ] && return 0
  done
  ext=".${name##*.}"
  for item in "${TEXT_EXTENSIONS[@]}"; do
    [ "$ext" = "$item" ] && return 0
  done
  return 1
}

substitute_name() {
  local in="$1"
  in="${in//applicationname_snake/$SNAKE_CASE_SERVICE_NAME}"
  in="${in//applicationname/$KEBAB_CASE_SERVICE_NAME}"
  in="${in//ApplicationName/$DOT_CASE_SERVICE_NAME}"
  printf '%s' "$in"
}

substitute_file_contents() {
  local file_path="$1"
  local tmp_file="$file_path.tmp"
  sed -e "s/{{NAMESPACE}}/$NAMESPACE/g" \
      -e "s/{{RABBITMQ_HOST}}/$RABBITMQ_HOST/g" \
      -e "s/{{RABBITMQ_CLUSTER_NAMESPACE}}/$RABBITMQ_CLUSTER_NAMESPACE/g" \
      -e "s/{{GATEWAY_NAMESPACE}}/$GATEWAY_NAMESPACE/g" \
      -e "s/{{GATEWAY_NAME}}/$GATEWAY_NAME/g" \
      -e "s/{{DOMAIN}}/$DOMAIN/g" \
      -e "s/applicationname_snake/$SNAKE_CASE_SERVICE_NAME/g" \
      -e "s/applicationname/$KEBAB_CASE_SERVICE_NAME/g" \
      -e "s/ApplicationName/$DOT_CASE_SERVICE_NAME/g" \
      "$file_path" > "$tmp_file"
  mv "$tmp_file" "$file_path"
}

copy_template_tree() {
  local src="$1" dst="$2"
  mkdir -p "$dst"
  cp -R "$src/." "$dst/"
  local find_expr=()
  for d in "${EXCLUDED_DIRS[@]}"; do
    [ ${#find_expr[@]} -gt 0 ] && find_expr+=("-o")
    find_expr+=("-name" "$d")
  done
  find "$dst" -depth \( "${find_expr[@]}" \) -type d -exec rm -rf {} +
}

process_template_tree() {
  local root="$1"
  # Content pass: substitute file contents (paths still hold placeholders).
  find "$root" -type f -print0 | while IFS= read -r -d '' file_path; do
    local base
    base=$(basename "$file_path")
    if is_text_target "$base"; then
      substitute_file_contents "$file_path"
    fi
  done

  # Rename pass: bottom-up so a parent rename doesn't invalidate child paths
  # already in the walk.
  find "$root" -depth -mindepth 1 | while IFS= read -r path; do
    local dir base new_base
    dir=$(dirname "$path")
    base=$(basename "$path")
    new_base=$(substitute_name "$base")
    if [ "$new_base" != "$base" ]; then
      mv "$path" "$dir/$new_base"
    fi
  done
}

for kind in shared worker api web; do
  echo "Generating src/$kind"
  src_dir="$TEMPLATES_SOURCE/$kind"
  dst_dir="$PROJECT_FOLDER/src/$kind"
  if [ ! -d "$src_dir" ]; then
    echo "Template $kind not found at $src_dir" >&2
    exit 1
  fi
  copy_template_tree "$src_dir" "$dst_dir"
  process_template_tree "$dst_dir"
done

echo "Generating k8s"
cp -R "$K8S_SOURCE" "$PROJECT_FOLDER"
find "$PROJECT_FOLDER/k8s" -type f -print0 | while IFS= read -r -d '' file_path; do
  substitute_file_contents "$file_path"
done

KRUN_JSON_PATH="$PROJECT_FOLDER/krun.json"
cp "$TEMPLATES_SOURCE/krun.json" "$KRUN_JSON_PATH"
substitute_file_contents "$KRUN_JSON_PATH"
echo "Created krun.json: $KRUN_JSON_PATH"
