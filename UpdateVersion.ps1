param (
    [Parameter(Mandatory=$true)][string]$solutionDir
)
$solutionDir = $solutionDir.TrimEnd('\')
$VersionMasterPath = Join-Path -Path $solutionDir -ChildPath "VersionMaster.txt"

# バージョン番号をファイルから変数に入れる
try {
    $newVersion = Get-Content -Path "$VersionMasterPath" -Raw
}
catch {
    Write-Error "エラーが発生しました（ファイル: $VersionMasterPath）: $_"
}
echo "バージョンは$newVersion"

# 置換情報を配列として定義
$replacements = @(
    @{ FilePath = "setting.yaml"; RegexPattern = "^Version: v`\d+`\.`\d+`\.`\d+"; Replacement = "Version: v$newVersion" }
)


# 各置換情報をループで処理

foreach ($item in $replacements) {
    $filePath = Join-Path -Path $solutionDir -ChildPath $item.FilePath
    $regexPattern = $item.RegexPattern
    $replacement = $item.Replacement

    try {
        # ファイルの内容を読み込み
        $contentBytes = [System.IO.File]::ReadAllBytes($filePath)
        $content = [System.Text.Encoding]::UTF8.GetString($contentBytes)
        
        # BOMがあれば除去
        if ($content.StartsWith([char]0xFEFF)) {
            $content = $content.Substring(1)
        }

        # デバッグ用: ファイル内容を表示
        # Write-Host "ファイル内容: '$content'"

        # 元の改行コードを検出
        $lineEnding = if ($content -match "`r`n") { "`r`n" } elseif ($content -match "`n") { "`n" } else { "`r" }
        
        # 正規表現がマッチするか確認
        if ($content -match $regexPattern) {
            Write-Host "正規表現にマッチしました: $regexPattern"
            $resolvedReplacement = $ExecutionContext.InvokeCommand.ExpandString($replacement)
            Write-Host "置換文字列: '$resolvedReplacement'"
            $modifiedContent = [regex]::Replace($content, $regexPattern, $resolvedReplacement)
        }
        else {
            Write-Host "正規表現にマッチしませんでした: $regexPattern"
            $modifiedContent = $content  # マッチしない場合は変更なし
        }
        
        # デバッグ用: 置換後の内容を表示
        # Write-Host "置換後: '$modifiedContent'"
        
        # ファイルに書き込み
        [System.IO.File]::WriteAllText($filePath, $modifiedContent, [System.Text.Encoding]::UTF8)
        
        Write-Host "ファイルの置換が完了しました: $filePath"
    }
    catch {
        Write-Error "エラーが発生しました（ファイル: $filePath）: $_"
    }
}