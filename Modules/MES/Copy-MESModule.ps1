param(
    [string]$SourceModule,   # e.g. "BaseData"
    [string]$TargetLayer     # e.g. "Domain" - the MES target layer
)

$sourceRoot = "Modules\$SourceModule"
$prefix = "Lzq.$SourceModule"
$targetPrefix = "Lzq.MES"

# Find all project directories under the source module
$projects = Get-ChildItem -Path $sourceRoot -Directory | Where-Object { $_.Name -like "$prefix.*" }

foreach ($proj in $projects) {
    $layer = $proj.Name -replace "^$prefix\.", ""
    $targetProjName = "$targetPrefix.$layer"
    $targetRoot = "Modules\MES\$targetProjName"

    Write-Host "  Copying $($proj.FullName) -> $targetRoot"

    $csFiles = Get-ChildItem -Path $proj.FullName -Recurse -Filter "*.cs"
    foreach ($file in $csFiles) {
        $content = Get-Content $file.FullName -Raw

        # Replace namespaces: longest matches first to avoid partial matches
        $content = $content -replace 'Lzq\.BaseData\.Application\.Contracts', 'Lzq.MES.Application.Contracts'
        $content = $content -replace 'Lzq\.Dashboard\.Application\.Contracts', 'Lzq.MES.Application.Contracts'
        $content = $content -replace 'Lzq\.Equipment\.Application\.Contracts', 'Lzq.MES.Application.Contracts'
        $content = $content -replace 'Lzq\.QA\.Application\.Contracts', 'Lzq.MES.Application.Contracts'
        $content = $content -replace 'Lzq\.WorkOrder\.Application\.Contracts', 'Lzq.MES.Application.Contracts'
        $content = $content -replace 'Lzq\.BaseData\.Application', 'Lzq.MES.Application'
        $content = $content -replace 'Lzq\.Dashboard\.Application', 'Lzq.MES.Application'
        $content = $content -replace 'Lzq\.Equipment\.Application', 'Lzq.MES.Application'
        $content = $content -replace 'Lzq\.QA\.Application', 'Lzq.MES.Application'
        $content = $content -replace 'Lzq\.WorkOrder\.Application', 'Lzq.MES.Application'
        $content = $content -replace 'Lzq\.BaseData\.Domain', 'Lzq.MES.Domain'
        $content = $content -replace 'Lzq\.Dashboard\.Domain', 'Lzq.MES.Domain'
        $content = $content -replace 'Lzq\.Equipment\.Domain', 'Lzq.MES.Domain'
        $content = $content -replace 'Lzq\.QA\.Domain', 'Lzq.MES.Domain'
        $content = $content -replace 'Lzq\.WorkOrder\.Domain', 'Lzq.MES.Domain'

        $relative = $file.FullName.Substring($proj.FullName.Length + 1)
        $target = Join-Path $targetRoot $relative
        $dir = Split-Path $target -Parent
        if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
        Set-Content -Path $target -Value $content -Encoding UTF8
    }
}

Write-Host "Done: $SourceModule -> MES"
