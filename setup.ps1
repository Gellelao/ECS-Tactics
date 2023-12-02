#Run from git bash to avoid error extracting tar

# Exit immediately on error
$ErrorActionPreference = "Stop"

# Create the libs directory if it does not exist
New-Item -ItemType Directory -Force -Path ./libs | Out-Null
Set-Location -Path ./libs

# Downloading fnalibs
Write-Host -ForegroundColor Green "Downloading fnalibs..."
Invoke-WebRequest -Uri "https://fna.flibitijibibo.com/archive/fnalibs.tar.bz2" -OutFile "fnalibs.tar.bz2"

# Extracting fnalibs
Write-Host -ForegroundColor Green "Extracting fnalibs..."
tar -xf "fnalibs.tar.bz2"

# Return to the parent directory
Set-Location -Path ..

# Update Git submodules
git submodule update --init --recursive