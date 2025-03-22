param (
    [string]$exePath,   # .exe�̃t���p�X
    [string]$yamlPath   # setting.yaml�̃t���p�X
)

# �A�Z���u������o�[�W���������擾
$assembly = [System.Reflection.Assembly]::LoadFrom($exePath)
$version = $assembly.GetName().Version
$newVersion = "v$($version.Major).$($version.Minor).$($version.Build)"

# setting.yaml�̓��e��ǂݍ���
$yamlContent = Get-Content -Path $yamlPath -Raw

# "Version: vX.Y.Z" �̍s��u���i���K�\���� vX.Y.Z �̕������}�b�`�j
$updatedContent = $yamlContent -replace "^(Version:).+", "`$1 $newVersion"

# �t�@�C���ɏ����߂�
$updatedContent.TrimEnd() | Set-Content -Path $yamlPath -Encoding UTF8

# �m�F�p�ɏo�́i�I�v�V�����j
Write-Output "Updated Version to: $newVersion"