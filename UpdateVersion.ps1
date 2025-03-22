param (
    [string]$exePath,   # .exeのフルパス
    [string]$yamlPath   # setting.yamlのフルパス
)

# アセンブリからバージョン情報を取得
$assembly = [System.Reflection.Assembly]::LoadFrom($exePath)
$version = $assembly.GetName().Version
$newVersion = "v$($version.Major).$($version.Minor).$($version.Build)"

# setting.yamlの内容を読み込み
$yamlContent = Get-Content -Path $yamlPath -Raw

# "Version: vX.Y.Z" の行を置換（正規表現で vX.Y.Z の部分をマッチ）
$updatedContent = $yamlContent -replace "^(Version:).+", "`$1 $newVersion"

# ファイルに書き戻し
$updatedContent.TrimEnd() | Set-Content -Path $yamlPath -Encoding UTF8

# 確認用に出力（オプション）
Write-Output "Updated Version to: $newVersion"