#!/usr/bin/env pwsh

# DayZ Complete Source - Build Script
# Este script compila toda a solução e gera o executável

Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  DayZ 1.25 - Build Script             ║" -ForegroundColor Cyan
Write-Host "║  Visual Studio Solution Builder       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Cyan

$solutionPath = ".\DayZ.sln"
$buildConfig = "Release"
$runtime = "win-x64"

# Verificar se a solução existe
if (-not (Test-Path $solutionPath)) {
    Write-Host "❌ Erro: Arquivo $solutionPath não encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "`n📦 Step 1: Restaurar dependências NuGet..." -ForegroundColor Yellow
dotnet restore $solutionPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha ao restaurar dependências!" -ForegroundColor Red
    exit 1
}

Write-Host "`n🔨 Step 2: Compilar solução ($buildConfig)..." -ForegroundColor Yellow
dotnet build $solutionPath --configuration $buildConfig --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha na compilação!" -ForegroundColor Red
    exit 1
}

Write-Host "`n📦 Step 3: Publicar Launcher como executável..." -ForegroundColor Yellow
$launcherProject = ".\src\Launcher\DayZ.Launcher.csproj"
$publishOutput = ".\build\Launcher"

dotnet publish $launcherProject `
    -c $buildConfig `
    -r $runtime `
    --self-contained `
    --output $publishOutput

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha ao publicar Launcher!" -ForegroundColor Red
    exit 1
}

Write-Host "`n📋 Step 4: Gerar informações de build..." -ForegroundColor Yellow

$buildInfo = @{
    "Build Date" = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "Configuration" = $buildConfig
    "Runtime" = $runtime
    "Solution" = $solutionPath
    "Output Path" = $publishOutput
    "Executable" = "$publishOutput\DayZLauncher.exe"
    "Status" = "SUCCESS"
}

$buildInfo | ConvertTo-Json | Out-File ".\build\BUILD_INFO.json" -Encoding UTF8

Write-Host "`n✅ BUILD COMPLETADO COM SUCESSO!" -ForegroundColor Green
Write-Host "`n📊 Informações do Build:"
$buildInfo | ForEach-Object {
    $_.GetEnumerator() | ForEach-Object {
        Write-Host "   $($_.Name): $($_.Value)" -ForegroundColor Cyan
    }
}

Write-Host "`n🎮 Próximo passo: Execute o launcher!" -ForegroundColor Green
Write-Host "   & '$publishOutput\DayZLauncher.exe'" -ForegroundColor Yellow

Write-Host "`n💻 Para compilação Debug (desenvolvimento):"
Write-Host "   dotnet build DayZ.sln --configuration Debug" -ForegroundColor Yellow

Write-Host "`n" 
