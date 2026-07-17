# Starts all six microservices, each in its own terminal window.
# Run from the backend folder:  .\run-all-services.ps1
$services = @("Identity", "Catalog", "Inventory", "Basket", "Ordering", "Notification")

foreach ($service in $services) {
    $project = "src/Services/$service/$service.Api"
    Write-Host "Starting $service.Api ..." -ForegroundColor Cyan
    Start-Process -FilePath "dotnet" -ArgumentList "run --project $project" -WorkingDirectory $PSScriptRoot
    Start-Sleep -Seconds 2
}

Write-Host ""
Write-Host "All services starting. Ports: Identity 5001, Catalog 5002, Inventory 5003, Basket 5004, Ordering 5005, Notification 5006" -ForegroundColor Green
