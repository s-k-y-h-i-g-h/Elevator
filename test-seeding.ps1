# Quick test script to verify database seeding
Write-Host "Testing database seeding..." -ForegroundColor Green

# Start the application in background and capture output
$job = Start-Job -ScriptBlock {
    Set-Location "C:\Users\G\Documents\Dev\Elevator\Elevator.Web"
    dotnet run --no-build 2>&1
}

# Wait a few seconds for startup
Start-Sleep -Seconds 8

# Stop the job
Stop-Job $job
$output = Receive-Job $job
Remove-Job $job

# Display relevant output
Write-Host "Application output:" -ForegroundColor Yellow
$output | Where-Object { $_ -match "(Database|Users|Compounds|Plants|Protocols|Discussions|Ratings|seeding|error)" } | ForEach-Object {
    Write-Host $_ -ForegroundColor Cyan
}

Write-Host "Test completed." -ForegroundColor Green