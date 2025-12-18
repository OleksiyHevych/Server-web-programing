Write-Host "Starting ACTORS API on port 7101..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "cd actors-api\actors-api; dotnet run" 

Start-Sleep -Milliseconds 500

Write-Host "Starting MOVIES API on port 7102..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "cd movies-api\movies-api; dotnet run"

Start-Sleep -Milliseconds 500

Write-Host "Starting MOVIE-ACTORS API on port 7103..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "cd movie-actors-api\movie-actors-api; dotnet run"

Start-Sleep -Milliseconds 500

Write-Host "Starting AUTH API on port 9000..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "cd auth-api\auth-api; dotnet run"

Start-Sleep -Milliseconds 500

Write-Host "Starting API-GATEWAY on port 7000..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "cd api-gateway\api-gateway; dotnet run"

Write-Host ""
Write-Host "All services started!" -ForegroundColor Green
Write-Host ""
Write-Host "→ ACTORS:        https://localhost:7101/swagger"
Write-Host "→ MOVIES:        http://localhost:7102/swagger"
Write-Host "→ MOVIE-ACTORS:  http://localhost:7103/swagger"
Write-Host "→ AUTH:          https://localhost:9000/swagger"
Write-Host "→ API GATEWAY:   https://localhost:7000/swagger"
