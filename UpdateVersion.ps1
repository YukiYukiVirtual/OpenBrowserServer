param (
    [Parameter(Mandatory=$true)][string]$solutionDir
)
$solutionDir = $solutionDir.TrimEnd('\')
$VersionMasterPath = Join-Path -Path $solutionDir -ChildPath "VersionMaster.txt"

# �o�[�W�����ԍ����t�@�C������ϐ��ɓ����
try {
    $newVersion = Get-Content -Path "$VersionMasterPath" -Raw
}
catch {
    Write-Error "�G���[���������܂����i�t�@�C��: $VersionMasterPath�j: $_"
}
echo "�o�[�W������$newVersion"

# �u������z��Ƃ��Ē�`
$replacements = @(
    @{ FilePath = "setting.yaml"; RegexPattern = "^Version: v`\d+`\.`\d+`\.`\d+"; Replacement = "Version: v$newVersion" }
)


# �e�u���������[�v�ŏ���

foreach ($item in $replacements) {
    $filePath = Join-Path -Path $solutionDir -ChildPath $item.FilePath
    $regexPattern = $item.RegexPattern
    $replacement = $item.Replacement

    try {
        # �t�@�C���̓��e��ǂݍ���
        $contentBytes = [System.IO.File]::ReadAllBytes($filePath)
        $content = [System.Text.Encoding]::UTF8.GetString($contentBytes)
        
        # BOM������Ώ���
        if ($content.StartsWith([char]0xFEFF)) {
            $content = $content.Substring(1)
        }

        # �f�o�b�O�p: �t�@�C�����e��\��
        # Write-Host "�t�@�C�����e: '$content'"

        # ���̉��s�R�[�h�����o
        $lineEnding = if ($content -match "`r`n") { "`r`n" } elseif ($content -match "`n") { "`n" } else { "`r" }
        
        # ���K�\�����}�b�`���邩�m�F
        if ($content -match $regexPattern) {
            Write-Host "���K�\���Ƀ}�b�`���܂���: $regexPattern"
            $resolvedReplacement = $ExecutionContext.InvokeCommand.ExpandString($replacement)
            Write-Host "�u��������: '$resolvedReplacement'"
            $modifiedContent = [regex]::Replace($content, $regexPattern, $resolvedReplacement)
        }
        else {
            Write-Host "���K�\���Ƀ}�b�`���܂���ł���: $regexPattern"
            $modifiedContent = $content  # �}�b�`���Ȃ��ꍇ�͕ύX�Ȃ�
        }
        
        # �f�o�b�O�p: �u����̓��e��\��
        # Write-Host "�u����: '$modifiedContent'"
        
        # �t�@�C���ɏ�������
        [System.IO.File]::WriteAllText($filePath, $modifiedContent, [System.Text.Encoding]::UTF8)
        
        Write-Host "�t�@�C���̒u�����������܂���: $filePath"
    }
    catch {
        Write-Error "�G���[���������܂����i�t�@�C��: $filePath�j: $_"
    }
}