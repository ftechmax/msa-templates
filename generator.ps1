$GeneratorVersion = "develop"
$ScriptDir = $PSScriptRoot
$ReleaseBaseUrl = "https://github.com/ftechmax/msa-templates/releases/download"

function Read-AnswerLine {
    param ([string]$PromptText)
    Write-Host -NoNewline "${PromptText}: "
    return [Console]::In.ReadLine()
}

function Prompt-Required {
    param (
        [string]$PromptText,
        [string]$Default
    )
    while ($true) {
        $display = if ($Default) { "$PromptText [$Default]" } else { $PromptText }
        $input = Read-AnswerLine $display
        if ($null -eq $input) {
            if ($Default) { return $Default }
            throw "Unexpected end of input."
        }
        $value = if ([string]::IsNullOrWhiteSpace($input)) { $Default } else { $input }
        if ([string]::IsNullOrWhiteSpace($value)) {
            Write-Host "  A value is required."
            continue
        }
        return $value
    }
}

function Prompt-PascalCase {
    param (
        [string]$PromptText
    )
    while ($true) {
        $input = Read-AnswerLine $PromptText
        if ($null -eq $input) {
            throw "Unexpected end of input."
        }
        if ([string]::IsNullOrWhiteSpace($input)) {
            Write-Host "  A value is required."
            continue
        }
        if ($input -notmatch '^[A-Z][a-zA-Z0-9]+$') {
            Write-Host "  Must be PascalCase (e.g., AwesomeApp)."
            continue
        }
        return $input
    }
}

function Prompt-Host {
    param (
        [string]$PromptText,
        [string]$Default
    )
    while ($true) {
        $input = Read-AnswerLine "$PromptText [$Default]"
        $value = if ([string]::IsNullOrWhiteSpace($input)) { $Default } else { $input }
        if ($value -notmatch '^[a-zA-Z0-9-]+\.[a-zA-Z0-9-]+\.svc$') {
            Write-Host "  Must match <name>.<namespace>.svc format."
            continue
        }
        return $value
    }
}

Write-Host "MSA Generator $GeneratorVersion"
Write-Host "-------------------"

$DestinationFolder = Prompt-Required  -PromptText "Destination folder" -Default "C:/git"
$ServiceName       = Prompt-PascalCase -PromptText "Service name (PascalCase)"
$Namespace         = Prompt-Required  -PromptText "Kubernetes namespace" -Default "default"
$RabbitmqHost      = Prompt-Host      -PromptText "RabbitMQ host" -Default "rabbitmq.rabbitmq-system.svc"
$FerretdbHost      = Prompt-Host      -PromptText "FerretDB host" -Default "ferretdb.ferretdb-system.svc"
$GatewayNamespace  = Prompt-Required  -PromptText "Istio Gateway namespace" -Default "istio-ingress"
$GatewayName       = Prompt-Required  -PromptText "Istio Gateway name" -Default "gateway"
$Domain            = Prompt-Required  -PromptText "Base domain" -Default "kube.local"

# Extract namespace from RabbitMQ host (rabbitmq.rabbitmq-system.svc -> rabbitmq-system)
$RabbitmqClusterNamespace = ($RabbitmqHost -split '\.')[1]

$kebabCaseServiceName = ($ServiceName -creplace '([A-Z]+)([A-Z][a-z])', '$1-$2' -creplace '([a-z0-9])([A-Z])', '$1-$2').toLower()
$dotCaseServiceName = ($ServiceName -creplace '([A-Z]+)([A-Z][a-z])', '$1.$2' -creplace '([a-z0-9])([A-Z])', '$1.$2')
$snakeCaseServiceName = $kebabCaseServiceName -replace '-', '_'

# Content-substitution allowlist; keep in sync between sh and ps1
$textExtensions = @(
    '.cs', '.csproj', '.sln', '.json', '.yaml', '.yml', '.ts',
    '.html', '.scss', '.css', '.md', '.conf', '.template',
    '.projitems', '.shproj', '.DotSettings'
)
$textNames = @('Dockerfile', '.dockerignore', '.editorconfig', '.gitignore')
$textSkipNames = @('package-lock.json')
$excludeDirs = @('bin', 'obj', 'dist', 'node_modules', '.angular', '.vs', '.vscode')

function Is-TextTarget {
    param ([string]$Name)
    if ($textSkipNames -contains $Name) { return $false }
    if ($textNames -contains $Name) { return $true }
    $ext = [System.IO.Path]::GetExtension($Name)
    return $textExtensions -contains $ext
}

function Substitute-Name {
    param ([string]$In)
    $out = $In `
        -creplace 'applicationname_snake', $snakeCaseServiceName `
        -creplace 'applicationname', $kebabCaseServiceName `
        -creplace 'ApplicationName', $dotCaseServiceName
    return $out
}

function Substitute-FileContents {
    param ([string]$FilePath)
    $content = Get-Content -Path $FilePath -Raw
    if ($null -eq $content) { return }
    $patched = $content `
        -creplace '\{\{NAMESPACE\}\}', $Namespace `
        -creplace '\{\{RABBITMQ_HOST\}\}', $RabbitmqHost `
        -creplace '\{\{RABBITMQ_CLUSTER_NAMESPACE\}\}', $RabbitmqClusterNamespace `
        -creplace '\{\{FERRETDB_HOST\}\}', $FerretdbHost `
        -creplace '\{\{GATEWAY_NAMESPACE\}\}', $GatewayNamespace `
        -creplace '\{\{GATEWAY_NAME\}\}', $GatewayName `
        -creplace '\{\{DOMAIN\}\}', $Domain `
        -creplace 'applicationname_snake', $snakeCaseServiceName `
        -creplace 'applicationname', $kebabCaseServiceName `
        -creplace 'ApplicationName', $dotCaseServiceName
    Set-Content -Path $FilePath -Value $patched -NoNewline
}

function Copy-TemplateTree {
    param ([string]$Src, [string]$Dst)
    if (-not (Test-Path -Path $Dst)) {
        New-Item -ItemType Directory -Path $Dst -Force | Out-Null
    }
    Copy-Item -Path (Join-Path $Src '*') -Destination $Dst -Recurse -Force
    Get-ChildItem -Path $Dst -Directory -Recurse -Force |
        Where-Object { $excludeDirs -contains $_.Name } |
        Sort-Object { $_.FullName.Length } -Descending |
        ForEach-Object { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
}

function Process-TemplateTree {
    param ([string]$Root)
    # Content pass: substitute file contents (paths still hold placeholders).
    Get-ChildItem -Path $Root -File -Recurse -Force | ForEach-Object {
        if (Is-TextTarget $_.Name) {
            Substitute-FileContents -FilePath $_.FullName
        }
    }
    # Rename pass: bottom-up so a parent rename doesn't invalidate child paths
    # already in the walk.
    Get-ChildItem -Path $Root -Recurse -Force |
        Sort-Object { $_.FullName.Length } -Descending |
        ForEach-Object {
            $newName = Substitute-Name $_.Name
            if ($newName -ne $_.Name) {
                Rename-Item -Path $_.FullName -NewName $newName
            }
        }
}

# Resolve $templatesSource and $k8sSource
# 1. Sibling checkout / extracted bundle: templates/ + k8s/ next to script.
# 2. Auto-fetch: download msa-templates-v$GeneratorVersion.zip from the
#    matching release and extract to a tempdir.

$fetchTempDir = $null
try {
    $templatesLocalPath = Join-Path $ScriptDir 'templates'
    $k8sLocalPath = Join-Path $ScriptDir 'k8s'

    if ((Test-Path -Path $templatesLocalPath) -and (Test-Path -Path $k8sLocalPath)) {
        $templatesSource = $templatesLocalPath
        $k8sSource = $k8sLocalPath
        Write-Host "Using local templates: $templatesSource"
    } else {
        if ($GeneratorVersion -eq 'develop') {
            throw "No sibling templates/ or k8s/ found and GeneratorVersion is 'develop' (no matching release to fetch). Run from a checkout or an extracted release bundle."
        }
        $zipName = "msa-templates-v$GeneratorVersion.zip"
        $zipUrl = "$ReleaseBaseUrl/v$GeneratorVersion/$zipName"
        $fetchTempDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $fetchTempDir | Out-Null
        $zipPath = Join-Path $fetchTempDir $zipName
        Write-Host "Downloading $zipUrl"
        Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath -UseBasicParsing
        Expand-Archive -Path $zipPath -DestinationPath $fetchTempDir -Force
        $bundleTemplates = Get-ChildItem -Path $fetchTempDir -Recurse -Directory -Filter 'templates' | Select-Object -First 1
        if (-not $bundleTemplates) {
            throw "Could not locate templates/ inside $zipName"
        }
        $bundleRoot = $bundleTemplates.Parent.FullName
        $templatesSource = Join-Path $bundleRoot 'templates'
        $k8sSource = Join-Path $bundleRoot 'k8s'
        if (-not (Test-Path -Path $k8sSource)) {
            throw "k8s/ not found inside $zipName"
        }
    }

    $ProjectFolder = (Join-Path -Path $DestinationFolder -ChildPath $kebabCaseServiceName)
    if (-not (Test-Path -Path $ProjectFolder -ErrorAction SilentlyContinue)) {
        New-Item -ItemType Directory -Path $ProjectFolder | Out-Null
        Write-Host "Created project folder: $ProjectFolder"
    } else {
        Write-Host "Project folder already exists: $ProjectFolder"
    }

    foreach ($kind in @('shared', 'worker', 'api', 'web')) {
        Write-Host "Generating src/$kind"
        $srcDir = Join-Path $templatesSource $kind
        $dstDir = Join-Path $ProjectFolder "src/$kind"
        if (-not (Test-Path -Path $srcDir)) {
            throw "Template $kind not found at $srcDir"
        }
        Copy-TemplateTree -Src $srcDir -Dst $dstDir
        Process-TemplateTree -Root $dstDir
    }

    Write-Host "Generating k8s"
    Copy-Item -Path $k8sSource -Destination $ProjectFolder -Recurse -Force
    Get-ChildItem -Path (Join-Path $ProjectFolder 'k8s') -Recurse -File | ForEach-Object {
        Substitute-FileContents -FilePath $_.FullName
    }

    $krunJsonPath = (Join-Path -Path $ProjectFolder -ChildPath 'krun.json')
    Copy-Item -Path (Join-Path $templatesSource 'krun.json') -Destination $krunJsonPath -Force
    Substitute-FileContents -FilePath $krunJsonPath
    Write-Host "Created krun.json: $krunJsonPath"

} finally {
    if ($fetchTempDir -and (Test-Path -Path $fetchTempDir)) {
        Remove-Item -Path $fetchTempDir -Recurse -Force
    }
}
