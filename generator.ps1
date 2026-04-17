$GeneratorVersion = "develop"
$ScriptDir = $PSScriptRoot

function Prompt-Required {
    param (
        [string]$PromptText,
        [string]$Default
    )
    while ($true) {
        if ($Default) {
            $input = Read-Host "$PromptText [$Default]"
        } else {
            $input = Read-Host "$PromptText"
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
        $input = Read-Host "$PromptText"
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
        $input = Read-Host "$PromptText [$Default]"
        $value = if ([string]::IsNullOrWhiteSpace($input)) { $Default } else { $input }
        if ($value -notmatch '^[a-zA-Z0-9-]+\.[a-zA-Z0-9-]+\.svc$') {
            Write-Host "  Must match <name>.<namespace>.svc format."
            continue
        }
        return $value
    }
}

# Interactive configuration
Write-Host "MSA Generator $GeneratorVersion"
Write-Host "-------------------"

$DestinationFolder = Prompt-Required  -PromptText "Destination folder" -Default "C:/git"
$ServiceName       = Prompt-PascalCase -PromptText "Service name (PascalCase)"
$Namespace         = Prompt-Required  -PromptText "Kubernetes namespace" -Default "default"
$RabbitmqHost      = Prompt-Host      -PromptText "RabbitMQ host" -Default "rabbitmq.rabbitmq-system.svc"
$FerretdbHost      = Prompt-Host      -PromptText "FerretDB host" -Default "ferretdb.ferretdb-system.svc"

# Extract namespace from RabbitMQ host (e.g. rabbitmq.rabbitmq-system.svc -> rabbitmq-system)
$RabbitmqClusterNamespace = ($RabbitmqHost -split '\.')[1]

# Prepare service name
$kebabCaseServiceName = ($ServiceName -creplace '([A-Z]+)([A-Z][a-z])', '$1-$2' -creplace '([a-z0-9])([A-Z])', '$1-$2').toLower()
$dotCaseServiceName = ($ServiceName -creplace '([A-Z]+)([A-Z][a-z])', '$1.$2' -creplace '([a-z0-9])([A-Z])', '$1.$2')
$snakeCaseServiceName = $kebabCaseServiceName -replace '-', '_'

# Install matching template version
$csproj = Join-Path (Join-Path $ScriptDir "src") "MSA.Templates.csproj"
if (Test-Path -Path $csproj) {
    Write-Host "Local source detected, packing templates..."
    Remove-Item -Path (Join-Path (Join-Path $ScriptDir "artifacts") "MSA.Templates.*.nupkg") -ErrorAction SilentlyContinue
    dotnet pack $csproj -o (Join-Path $ScriptDir "artifacts") --nologo -v quiet
    $nupkg = Get-ChildItem -Path (Join-Path $ScriptDir "artifacts") -Filter "MSA.Templates.*.nupkg" | Select-Object -First 1
    # Uninstall any existing version to avoid conflicts
    dotnet new uninstall MSA.Templates 2>$null
    Write-Host "Installing $($nupkg.FullName)"
    dotnet new install $nupkg.FullName
} else {
    Write-Host "Installing MSA.Templates v$GeneratorVersion"
    dotnet new uninstall MSA.Templates 2>$null
    dotnet new install "MSA.Templates@$GeneratorVersion"
}

# Resolve k8s source folder
$tempK8sDir = $null

try {

    $k8sLocalPath = Join-Path $ScriptDir "k8s"
    if (Test-Path -Path $k8sLocalPath) {
        $k8sSource = $k8sLocalPath
    } else {
        $nugetPackages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $HOME ".nuget/packages" }
        $k8sSource = Join-Path (Join-Path (Join-Path $nugetPackages "msa.templates") $GeneratorVersion) "k8s"
        $templatePackage = $null

        if (-not (Test-Path -Path $k8sSource)) {
            $templateHome = if ($env:DOTNET_CLI_HOME) { $env:DOTNET_CLI_HOME } else { $HOME }
            $templatePackage = Join-Path (Join-Path (Join-Path $templateHome ".templateengine") "packages") "MSA.Templates.$GeneratorVersion.nupkg"
            if (Test-Path -Path $templatePackage) {
                $tempK8sDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
                New-Item -ItemType Directory -Path $tempK8sDir | Out-Null
                Expand-Archive -Path $templatePackage -DestinationPath $tempK8sDir -Force
                $k8sSource = Join-Path $tempK8sDir "k8s"
            }
        }

        if (-not (Test-Path -Path $k8sSource)) {
            throw "k8s manifests not found in $k8sSource or $templatePackage"
        }
    }

# Prepare project folder
$ProjectFolder = (Join-Path -Path $DestinationFolder -ChildPath $kebabCaseServiceName)
if (-not (Test-Path -Path $ProjectFolder -ErrorAction SilentlyContinue)) {
    New-Item -ItemType Directory -Path $ProjectFolder | Out-Null
    Write-Host "Created project folder: $ProjectFolder"
} else {
    Write-Host "Project folder already exists: $ProjectFolder"
}

# Generate projects
Write-Host "Generating src/shared"
dotnet new msa-shared -n $dotCaseServiceName -o (Join-Path -Path $ProjectFolder -ChildPath 'src/shared')
Write-Host "Generating src/worker"
dotnet new msa-worker -n $dotCaseServiceName -o (Join-Path -Path $ProjectFolder -ChildPath 'src/worker')
Write-Host "Generating src/api"
dotnet new msa-api -n $dotCaseServiceName -o (Join-Path -Path $ProjectFolder -ChildPath 'src/api')
Write-Host "Generating src/web"
dotnet new msa-web -n $dotCaseServiceName -o (Join-Path -Path $ProjectFolder -ChildPath 'src/web')

# Patch k8s folder
Copy-Item -Path $k8sSource -Destination $ProjectFolder -Recurse -Force
Get-ChildItem -Path "$ProjectFolder/k8s" -Recurse | ForEach-Object {
    if (-not $_.PSIsContainer) {
        $filePath = $_.FullName
        Write-Host "Patching $filePath"

        (Get-Content -Path $filePath) `
            -creplace '{{NAMESPACE}}', $Namespace `
            -creplace '{{RABBITMQ_HOST}}', $RabbitmqHost `
            -creplace '{{RABBITMQ_CLUSTER_NAMESPACE}}', $RabbitmqClusterNamespace `
            -creplace '{{FERRETDB_HOST}}', $FerretdbHost `
            -creplace 'applicationname_snake', $snakeCaseServiceName `
            -creplace 'applicationname', $kebabCaseServiceName `
            -creplace 'ApplicationName', $dotCaseServiceName `
        | Set-Content -Path $filePath
    }
}

# Create krun.json
$krunJsonPath = (Join-Path -Path $ProjectFolder -ChildPath 'krun.json')
$krunJsonContent = @"
[
    {
        "name": "applicationname-worker",
        "path": "src/worker",
        "dockerfile": "ApplicationName.Worker",
        "context": "src/",
        "intercept_port": 5001,
        "service_dependencies": [
            { "host": "applicationname-cache", "port": 6379 },
            { "host": "$RabbitmqHost", "port": 5672 },
            { "host": "$FerretdbHost", "port": 27017 }
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
            { "host": "$RabbitmqHost", "port": 5672 }
        ]
    },
    {
        "name": "applicationname-web",
        "path": "src/web",
        "dockerfile": ".",
        "context": "src/web"
    }
]
"@

$krunJsonContent = $krunJsonContent `
    -creplace 'applicationname', $kebabCaseServiceName `
    -creplace 'ApplicationName', $dotCaseServiceName

$krunJsonContent | Set-Content -Path $krunJsonPath
Write-Host "Created krun.json: $krunJsonPath"

} finally {
    if ($tempK8sDir -and (Test-Path -Path $tempK8sDir)) {
        Remove-Item -Path $tempK8sDir -Recurse -Force
    }
}
