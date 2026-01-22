Write-Host "Waiting for LocalStack to be ready..."
$maxAttempts = 30
$attempt = 0

while ($attempt -lt $maxAttempts) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:4566/_localstack/health" -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.Content -match '"ses": "available"') {
            Write-Host "LocalStack SES service is ready!"
            break
        }
    }
    catch {
        # Continue waiting
    }
    
    $attempt++
    Start-Sleep -Seconds 2
}

if ($attempt -eq $maxAttempts) {
    Write-Host "LocalStack SES service did not become available in time."
    exit 1
}

Write-Host "Verifying email address in LocalStack SES..."
aws --endpoint-url=http://localhost:4566 ses verify-email-identity --email-address noreply@example.com --region us-east-1

if ($LASTEXITCODE -eq 0) {
    Write-Host "Email address noreply@example.com verified successfully!"
}
else {
    Write-Host "Failed to verify email address. It may already be verified."
}
