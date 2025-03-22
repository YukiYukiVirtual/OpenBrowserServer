param (
    [string]$exePath,   # .exeのフルパス
    [string]$yamlPath   # setting.yamlのフルパス
)

# .exeが存在するか確認
if (-not (Test-Path $exePath)) {
    Write-Error "EXE file not found: $exePath"
    exit 1
}

# setting.yamlが存在するか確認
if (-not (Test-Path $yamlPath)) {
    Write-Error "setting.yaml not found: $yamlPath"
    exit 1
}

# バージョン取得
$assembly = [System.Reflection.Assembly]::LoadFrom($exePath)
$version = $assembly.GetName().Version
$newVersion = "v$($version.Major).$($version.Minor).$($version.Build)"

# YAMLを読み込み（行ごとに取得）
$yamlLines = Get-Content -Path $yamlPath

# 元の改行コードを検出
$rawContent = [System.IO.File]::ReadAllText($yamlPath)
if ($rawContent -match "\r\n") {
    $newline = "`r`n"  # CRLF
} else {
    $newline = "`n"    # LF
}

# Version行を更新
$updatedLines = $yamlLines | ForEach-Object {
    if ($_ -match "Version:\s*v\d+\.\d+\.\d+") {
        "Version: $newVersion"
    } else {
        $_
    }
}

# 改行コードを統一して書き込み
[System.IO.File]::WriteAllLines($yamlPath, $updatedLines, [System.Text.UTF8Encoding]::new($false)) # BOMなしUTF-8
# 改行コードを手動で適用する場合
$updatedContent = $updatedLines -join $newline
$updatedContent | Set-Content -Path $yamlPath -Encoding UTF8 -NoNewline

Write-Output "Updated Version to: $newVersion with consistent newline"