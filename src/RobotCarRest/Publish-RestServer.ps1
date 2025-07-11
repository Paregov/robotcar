param(
    [switch]$UploadToCar
)

<#
.SYNOPSIS
    Zips the contents of a specified folder into a single archive file.

.DESCRIPTION
    This function compresses all files and subfolders from a given source directory into a .zip file.
    It does not include the top-level source folder itself in the archive, only its contents.
    You can specify a destination path for the archive. If you don't, the archive will be created
    in the parent directory of the source folder with the same name and a .zip extension.

.PARAMETER SourcePath
    The path to the folder whose contents you want to zip. This parameter is mandatory.

.PARAMETER DestinationPath
    The path for the output .zip file. This can be a path to a file or a directory.
    If it's a directory, the zip file will be created inside it with the source folder's name.
    If omitted, the zip file is created alongside the source folder.

.PARAMETER Force
    If specified, this switch will overwrite the destination .zip file if it already exists.
    Without this switch, the function will stop with an error to prevent accidental overwrites.

.EXAMPLE
    PS C:\> New-ZipFromFolderContents -SourcePath "C:\MyProject\build_output"

    Description:
    This command will take all the files and folders inside "C:\MyProject\build_output" and
    create a zip file named "build_output.zip" located in "C:\MyProject".

.EXAMPLE
    PS C:\> New-ZipFromFolderContents -SourcePath "C:\Users\Me\Documents" -DestinationPath "C:\Backups\docs_backup.zip"

    Description:
    This command zips the contents of the Documents folder and saves the archive as "docs_backup.zip"
    in the "C:\Backups" directory.

.EXAMPLE
    PS C:\> Get-Item "C:\MyProject\build_output" | New-ZipFromFolderContents -Force

    Description:
    This command demonstrates pipeline input. It gets the directory object for "build_output" and
    pipes it to the function. It will create "build_output.zip" in "C:\MyProject" and overwrite it
    if it already exists.
#>
function New-ZipFromFolderContents
{
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0)]
        [string]$SourcePath,

        [Parameter(Mandatory = $false, Position = 1)]
        [string]$DestinationPath,

        [Parameter(Mandatory = $false)]
        [switch]$Force
    )

    try
    {
        $resolvedSourcePath = Resolve-Path -Path $SourcePath -ErrorAction Stop
        if (-not (Test-Path -Path $resolvedSourcePath -PathType Container))
        {
            throw "'$resolvedSourcePath' is not a directory."
        }
    }
    catch
    {
        Write-Error "Source path error: $_"
        return
    }

    if ([string]::IsNullOrEmpty($DestinationPath))
    {
        # If no destination is specified, create the zip in the parent directory
        $parentFolder = Split-Path -Path $resolvedSourcePath -Parent
        $folderName = Split-Path -Path $resolvedSourcePath -Leaf
        $resolvedDestinationPath = Join-Path -Path $parentFolder -ChildPath "$folderName.zip"
    }
    else
    {
        $resolvedDestinationPath = Resolve-Path -Path $DestinationPath -ErrorAction SilentlyContinue
        # If the provided destination is an existing directory, build the file path inside it
        if ($resolvedDestinationPath -and (Test-Path -Path $resolvedDestinationPath -PathType Container))
        {
             $folderName = Split-Path -Path $resolvedSourcePath -Leaf
             $resolvedDestinationPath = Join-Path -Path $resolvedDestinationPath -ChildPath "$folderName.zip"
        }
        else
        {
            # Otherwise, use the provided path as the full file path
            $resolvedDestinationPath = $DestinationPath
        }
    }

    if ((Test-Path -Path $resolvedDestinationPath) -and (-not $Force))
    {
        Write-Error "Destination file '$resolvedDestinationPath' already exists. Use the -Force switch to overwrite."
        return
    }

    $itemsToZip = Get-ChildItem -Path $resolvedSourcePath

    if ($null -eq $itemsToZip) {
        Write-Warning "Source folder '$resolvedSourcePath' is empty. Creating an empty archive."
        # Compress-Archive cannot create an empty zip from an empty folder.
        # As a workaround, we create a temporary empty directory to zip.
        $tempDir = New-Item -ItemType Directory -Path (Join-Path $env:TEMP ([guid]::NewGuid().ToString()))
        Compress-Archive -Path $tempDir.FullName -DestinationPath $resolvedDestinationPath -Force:$Force
        Remove-Item -Path $tempDir.FullName -Recurse
        Write-Host "Successfully created empty archive at '$resolvedDestinationPath'."
        return
    }

    if ($pscmdlet.ShouldProcess($resolvedDestinationPath, "Compressing contents of '$resolvedSourcePath'"))
    {
        try
        {
            # Use the .FullName property to pass the full paths of all items to the command
            Compress-Archive -Path $itemsToZip.FullName -DestinationPath $resolvedDestinationPath -Force:$Force -ErrorAction Stop
            Write-Host "Successfully created archive at '$resolvedDestinationPath'." -ForegroundColor Green
        }
        catch
        {
            Write-Error "Failed to create zip archive: $_"
        }
    }
}

$currentPath = $PSScriptRoot
try
{
    dotnet publish --configuration Release --runtime linux-arm64
    # If you want to have all included but the size is much bigger, you can use the following command:
    # dotnet publish --configuration Release --runtime linux-arm64 --self-contained true

    if ($UploadToCar)
    {
        Write-Host "Zipping contents of the Rest Server..."
        $releasePath = Join-Path -Path $currentPath -ChildPath "RobotCarRest.Service\bin\Release\net8.0\linux-arm64\publish\"
        $zipPath = Join-Path -Path $currentPath -ChildPath "RobotCarRest.Service.zip"
        New-ZipFromFolderContents -SourcePath $releasePath -DestinationPath $zipPath -Force

        Write-Host "Uploading to CAR..."
        # Assuming Upload-Car is a function defined elsewhere in your script

        Write-Host "Reading file content from '$zipPath'..."
        $fileBytes = [System.IO.File]::ReadAllBytes($zipPath)
        $fileStreamLength = $fileBytes.Length
        Write-Host "File size is $fileStreamLength bytes."
        
        $ApiUrl = "http://192.168.200.85:5000/api/v1/software/restserver"
        $headers = @{
            "Content-Type" = "application/octet-stream"
        }

        Write-Host "Sending POST request to '$ApiUrl' with file size $fileStreamLength bytes."
        $response = Invoke-RestMethod -Uri $ApiUrl -Method "POST" -Headers $headers -Body $fileBytes -ErrorAction Stop
        Write-Host "Upload completed successfully. Response: $response" -ForegroundColor Green
    }
}
catch
{
    Write-Error "An error occurred: $_"
}
finally
{
    # Cleanup if necessary
    Set-Location -Path $currentPath
}

