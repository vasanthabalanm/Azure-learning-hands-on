# Azure Function Test Script
# Prerequisites: Function running locally (func start), Azurite running

param(
    [ValidateSet("Email", "SMS", "Push", "All")]
    [string]$NotificationType = "Email",
    [int]$Count = 1
)

$FunctionUrl = "http://localhost:7071/api/notifications/send"
$TestUserId = "user-$(Get-Random -Minimum 100 -Maximum 999)"

function Test-Notification {
    param(
        [string]$Type,
        [int]$Index
    )

    $body = @{
        UserId = "$TestUserId-$Index"
        Email = "test$Index@example.com"
        PhoneNumber = "+1$(Get-Random -Minimum 1000000000 -Maximum 9999999999)"
        Subject = "Test $Type Notification #$Index"
        Body = "This is test notification number $Index of type $Type"
        NotificationType = $Type
    } | ConvertTo-Json

    Write-Host "`n📤 Sending $Type Notification ($Index/$Count)..." -ForegroundColor Cyan
    Write-Host "Payload: $body" -ForegroundColor Gray

    try {
        $response = Invoke-WebRequest -Uri $FunctionUrl `
            -Method Post `
            -Body $body `
            -ContentType "application/json" `
            -PassThru `
            -ErrorAction Stop

        $content = $response.Content | ConvertFrom-Json
        Write-Host "✅ Success (Status: $($response.StatusCode))" -ForegroundColor Green
        Write-Host "   MessageId: $($content.messageId)" -ForegroundColor Green
        Write-Host "   Status: $($content.status)" -ForegroundColor Green

        return $true
    }
    catch {
        Write-Host "❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Show-Menu {
    Write-Host "`n" -ForegroundColor Cyan
    Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║   Azure Function - Test Runner         ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host "`n"
}

# Main
Show-Menu
Write-Host "Testing $NotificationType notification(s)..." -ForegroundColor Yellow
Write-Host "Base UserId: $TestUserId" -ForegroundColor Yellow

$successCount = 0
$failureCount = 0

if ($NotificationType -eq "All") {
    foreach ($type in @("Email", "SMS", "Push")) {
        for ($i = 1; $i -le $Count; $i++) {
            $result = Test-Notification -Type $type -Index $i
            if ($result) { $successCount++ } else { $failureCount++ }
            Start-Sleep -Milliseconds 500
        }
    }
}
else {
    for ($i = 1; $i -le $Count; $i++) {
        $result = Test-Notification -Type $NotificationType -Index $i
        if ($result) { $successCount++ } else { $failureCount++ }
        Start-Sleep -Milliseconds 500
    }
}

# Summary
Write-Host "`n" -ForegroundColor Cyan
Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Test Summary                         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host "✅ Successful: $successCount" -ForegroundColor Green
Write-Host "❌ Failed: $failureCount" -ForegroundColor $(if ($failureCount -eq 0) { "Green" } else { "Red" })
Write-Host "`n📝 Check Terminal 2 logs to see queue processing!`n" -ForegroundColor Yellow
