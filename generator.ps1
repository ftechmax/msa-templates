param (
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)] 
    [string]$DestinationFolder = "C:/git",
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)] 
    [string]$ServiceName,
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)] 
    [string]$RabbitMqUserSecret,
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)] 
    [string]$MongoDbUserSecret,
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)] 
    [string]$RedisServiceName,
    [Parameter(Mandatory = $true, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)] 
    [string]$RedisServiceNamespace
)

# Prepare service name
$kebabCaseServiceName = ($ServiceName -creplace '([A-Z])', '-$1' -replace '^-', '' -replace ' ', '-').toLower()
Write-Host "Service name: $kebabCaseServiceName"

# Prepare project folder
$ProjectFolder = (Join-Path -Path $DestinationFolder -ChildPath $kebabCaseServiceName)
if (-not (Test-Path -Path $ProjectFolder -ErrorAction SilentlyContinue)) {
    New-Item -ItemType Directory -Path $ProjectFolder | Out-Null
    Write-Host "Created project folder: $ProjectFolder"
} else {
    Write-Host "Project folder already exists: $ProjectFolder"
}

# Generate projects
dotnet new msa-shared -n $ServiceName -o (Join-Path -Path $ProjectFolder -ChildPath 'src/shared')
dotnet new msa-worker -n $ServiceName -o (Join-Path -Path $ProjectFolder -ChildPath 'src/worker')
dotnet new msa-api -n $ServiceName -o (Join-Path -Path $ProjectFolder -ChildPath 'src/api')
dotnet new msa-web -n $ServiceName -o (Join-Path -Path $ProjectFolder -ChildPath 'src/web')

# Patch k8s folder
Copy-Item -Path "./k8s" -Destination $ProjectFolder -Recurse -Force
Get-ChildItem -Path "$ProjectFolder/k8s" -Recurse | ForEach-Object {
    if (-not $_.PSIsContainer) {
        $filePath = $_.FullName
        Write-Host "Patching $filePath"
        
        (Get-Content -Path $filePath) `
            -creplace '{{RABBITMQ-SECRET-NAME}}', $RabbitMqUserSecret `
            -creplace '{{MONGODB-SECRET-NAME}}', $MongoDbUserSecret `
            -creplace '{{REDIS-SERVICE}}', "$RedisServiceName.$RedisServiceNamespace.svc" `
            -creplace 'applicationname', $kebabCaseServiceName `
            -creplace 'ApplicationName', $ServiceName `
        | Set-Content -Path $filePath
    }
}

# Patch KubernetesLocalProcessConfig.yaml
$filePath = Join-Path -Path $ProjectFolder -ChildPath "src/api/$ServiceName.Api/KubernetesLocalProcessConfig.yaml"
(Get-Content -Path $filePath) -creplace '{{REDIS-SERVICE}}', "$RedisServiceName.$RedisServiceNamespace" | Set-Content -Path $filePath
Write-Host "Patching $filePath"