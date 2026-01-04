#!/usr/bin/env pwsh

param(
    [switch]$Down = $false,
    [switch]$Rebuild = $false,
    [switch]$Logs = $false,
    [switch]$Status = $false,
    [switch]$Clean = $false
)

$root = $PSScriptRoot
$ErrorActionPreference = "Stop"

# Garante que o arquivo .env exista (usando .env.template como base)
$envFile = Join-Path $root ".env"
$templateFile = Join-Path $root ".env.template"
if (-not (Test-Path $envFile)) {
    if (Test-Path $templateFile) {
        Copy-Item $templateFile $envFile -Force
        Write-Host "Arquivo .env não encontrado. Gerado a partir de .env.template. Atualize os segredos conforme necessário." -ForegroundColor Yellow
    }
    else {
        throw ".env e .env.template não encontrados em $root"
    }
}

switch ($true) {
    $Clean {
        docker-compose down -v --remove-orphans
        docker system prune -f
        docker volume prune -f
        exit 0
    }
    
    $Down {
        docker-compose down
        exit 0
    }
    
    $Status {
        docker-compose ps
        exit 0
    }
    
    $Logs {
        docker-compose logs -f
        exit 0
    }
}

try {
    if ($Rebuild) {
        docker-compose down
        docker rmi catalogapi:latest catalogapi-migrations:latest -f 2>$null
    }
    
    docker build -t catalogapi:latest --target runtime -f (Join-Path $root "FCG.Catalog.API/Dockerfile") $root
    docker build -t catalogapi-migrations:latest --target migrations -f (Join-Path $root "FCG.Catalog.API/Dockerfile") $root
    
    docker-compose up -d sqlserver rabbitmq
    
    Start-Sleep -Seconds 30
    
    docker-compose run --rm catalogapi-migrations database update --startup-project FCG.Catalog.API/FCG.Catalog.API.csproj --project FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj --configuration Release
    
    docker-compose up -d catalogapi
    
    Start-Sleep -Seconds 15
    
    docker-compose ps   
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -TimeoutSec 10
    }
    catch {
    }

}
catch {
    docker-compose logs --tail=50
    exit 1
}