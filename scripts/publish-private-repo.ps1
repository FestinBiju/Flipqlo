param(
    [Parameter(Mandatory = $true)]
    [string]$RepoName,

    [string]$Description = "Flipqlo Reborn - Windows .scr + Android DreamService"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) is not installed. Install it first: https://cli.github.com/"
}

$authStatus = gh auth status 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run: gh auth login"
}

$existingRemote = git remote get-url origin 2>$null
if ($LASTEXITCODE -eq 0 -and $existingRemote) {
    Write-Host "Remote 'origin' already exists: $existingRemote"
    Write-Host "Pushing current branch..."
    git push -u origin main
    exit 0
}

Write-Host "Creating private GitHub repository '$RepoName' and pushing main..."
gh repo create $RepoName --private --description $Description --source . --remote origin --push

Write-Host "Done. Private repository created and code pushed."
