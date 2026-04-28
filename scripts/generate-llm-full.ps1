<#
.SYNOPSIS
    Generates the llm-full.txt file from markdown files in the repository.
#>

# Move to the root directory
$ProjectRoot = Resolve-Path "$PSScriptRoot\.."
Set-Location $ProjectRoot

$OutputFile = "llm-full.txt"

# Empty the output file
Clear-Content $OutputFile -ErrorAction SilentlyContinue
if (-not (Test-Path $OutputFile)) {
    New-Item -ItemType File -Path $OutputFile | Out-Null
}

# Add README.md on top if it exists
if (Test-Path "README.md") {
    Add-Content -Path $OutputFile -Value "# README.md"
    Add-Content -Path $OutputFile -Value ""
    $content = Get-Content -Path "README.md" -Raw
    if ($content) {
        Add-Content -Path $OutputFile -Value $content
    }
    Add-Content -Path $OutputFile -Value ""
    Add-Content -Path $OutputFile -Value "---"
    Add-Content -Path $OutputFile -Value ""
}

# Find all markdown files, excluding specific folders and files
$files = Get-ChildItem -Path "." -Filter "*.md" -Recurse -File | 
         Where-Object { 
             $_.FullName -notmatch '[\\/](\.git|\.venv|bin|obj)[\\/]' -and 
             $_.Name -notmatch '(?i)^(readme\.md|contributing\.md|license\.md)$' 
         } | 
         Sort-Object FullName

foreach ($file in $files) {
    # Get relative path with forward slashes for consistency
    $relativePath = $file.FullName.Substring($ProjectRoot.Path.Length + 1).Replace('\', '/')
    
    Add-Content -Path $OutputFile -Value "# $relativePath"
    Add-Content -Path $OutputFile -Value ""
    
    # Read the content and append
    $content = Get-Content -Path $file.FullName -Raw
    if ($content) {
        Add-Content -Path $OutputFile -Value $content
    }
    
    Add-Content -Path $OutputFile -Value ""
    Add-Content -Path $OutputFile -Value "---"
    Add-Content -Path $OutputFile -Value ""
}

Write-Host "Successfully generated $OutputFile"
