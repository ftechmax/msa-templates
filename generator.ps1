param (
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [string]$DestinationFolder = "C:/git",
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [string]$ServiceName,
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [string]$RabbitMqUserSecret
)

$GeneratorVersion = "0.0.0"
$ScriptDir = $PSScriptRoot
$GitHubRepo = "ftechmax/msa-templates"

# Prepare service name
$kebabCaseServiceName = ($ServiceName -creplace '([A-Z])', '-$1' -replace '^-', '' -replace ' ', '-').toLower()
$dotCaseServiceName = ($ServiceName -creplace '([A-Z])', '.$1' -replace '^\.', '')
Write-Host "Service name: $kebabCaseServiceName"
Write-Host "Dot case name: $dotCaseServiceName"

# Install matching template version
Write-Host "Installing MSA.Templates v$GeneratorVersion"
dotnet new install "MSA.Templates::$GeneratorVersion"

# Resolve k8s source folder
$k8sLocalPath = Join-Path $ScriptDir "k8s"
if (Test-Path -Path $k8sLocalPath) {
    $k8sSource = $k8sLocalPath
} else {
    Write-Host "Downloading k8s manifests for v$GeneratorVersion..."
    $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
    New-Item -ItemType Directory -Path $tempDir | Out-Null
    $downloadUrl = "https://github.com/$GitHubRepo/releases/download/$GeneratorVersion/msa-generator-k8s-$GeneratorVersion.zip"
    $zipPath = Join-Path $tempDir "k8s.zip"
    Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath
    Expand-Archive -Path $zipPath -DestinationPath $tempDir
    $k8sSource = Join-Path $tempDir "k8s"
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
            -creplace '{{RABBITMQ-SECRET-NAME}}', $RabbitMqUserSecret `
            -creplace 'applicationname', $kebabCaseServiceName `
            -creplace 'ApplicationName', $dotCaseServiceName `
        | Set-Content -Path $filePath
    }
}

# Clean up temp directory if used
if ($tempDir -and (Test-Path -Path $tempDir)) {
    Remove-Item -Path $tempDir -Recurse -Force
}

# Create krun.json
$krunJsonPath = (Join-Path -Path $ProjectFolder -ChildPath 'krun.json')
$krunJsonContent = @'
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
'@

$krunJsonContent = $krunJsonContent `
    -creplace 'applicationname', $kebabCaseServiceName `
    -creplace 'ApplicationName', $dotCaseServiceName

$krunJsonContent | Set-Content -Path $krunJsonPath
Write-Host "Created krun.json: $krunJsonPath"
