# Copyright © Svetoslav Paregov. All rights reserved.
param(
    [switch]$SkipBuild,
    [switch]$UploadToCar
)

$currentFolder = Get-Location

try
{
    if (-not $SkipBuild)
    {
        if (!(Test-Path "build"))
        {
            New-Item -ItemType Directory -Name "build" | Out-Null
        }

        cd "build"
        Get-ChildItem | Remove-Item -Force -Recurse -ErrorAction Continue
        cmake ..
        ninja

        if ($LASTEXITCODE -ne 0)
        {
            Write-Host "CMake or Ninja build failed. Please check the output above for details."
            exit $LASTEXITCODE
        }
    }
}
catch
{
    Write-Host "An error occurred during the rebuild process."
    Write-Host "Please check the output above for details."
    Write-Host $_
}
finally
{
    Set-Location $currentFolder
    Write-Host "Rebuild process completed."
}

try
{
    if ($UploadToCar)
    {
        $releasePath = Join-Path -Path $currentFolder -ChildPath "build\LowLevelController.bin"

        Write-Host "Uploading to CAR..."
        # Assuming Upload-Car is a function defined elsewhere in your script

        Write-Verbose "Reading file content from '$releasePath'..."
        $fileBytes = [System.IO.File]::ReadAllBytes($releasePath)
        $fileStreamLength = $fileBytes.Length
        Write-Verbose "File size is $fileStreamLength bytes."
        
        $ApiUrl = "http://192.168.200.54:5000/api/v1/software/firmware"
        $headers = @{
            "Content-Type" = "application/octet-stream"
        }

        $response = Invoke-RestMethod -Uri $ApiUrl -Method "POST" -Headers $headers -Body $fileBytes -ErrorAction Stop
        Write-Host "Upload completed with response: $response" -ForegroundColor Green
    }
}
catch
{
    Write-Host "An error occurred during the upload process."
    Write-Host "Please check the output above for details."
    Write-Host $_
}