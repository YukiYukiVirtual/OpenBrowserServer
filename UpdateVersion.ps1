param (
    [string]$exePath,   # .exe�̃t���p�X
    [string]$yamlPath   # setting.yaml�̃t���p�X
)

# .exe�����݂��邩�m�F
if (-not (Test-Path $exePath)) {
    Write-Error "EXE file not found: $exePath"
    exit 1
}

# setting.yaml�����݂��邩�m�F
if (-not (Test-Path $yamlPath)) {
    Write-Error "setting.yaml not found: $yamlPath"
    exit 1
}

# �o�[�W�����擾
$assembly = [System.Reflection.Assembly]::LoadFrom($exePath)
$version = $assembly.GetName().Version
$newVersion = "v$($version.Major).$($version.Minor).$($version.Build)"

# YAML��ǂݍ��݁i�s���ƂɎ擾�j
$yamlLines = Get-Content -Path $yamlPath

# ���̉��s�R�[�h�����o
$rawContent = [System.IO.File]::ReadAllText($yamlPath)
if ($rawContent -match "\r\n") {
    $newline = "`r`n"  # CRLF
} else {
    $newline = "`n"    # LF
}

# Version�s���X�V
$updatedLines = $yamlLines | ForEach-Object {
    if ($_ -match "Version:\s*v\d+\.\d+\.\d+") {
        "Version: $newVersion"
    } else {
        $_
    }
}

# ���s�R�[�h�𓝈ꂵ�ď�������
[System.IO.File]::WriteAllLines($yamlPath, $updatedLines, [System.Text.UTF8Encoding]::new($false)) # BOM�Ȃ�UTF-8
# ���s�R�[�h���蓮�œK�p����ꍇ
$updatedContent = $updatedLines -join $newline
$updatedContent | Set-Content -Path $yamlPath -Encoding UTF8 -NoNewline

Write-Output "Updated Version to: $newVersion with consistent newline"