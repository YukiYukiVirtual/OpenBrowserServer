Add-Type -AssemblyName System.Windows.Forms

echo "---�n���J�n---"
[void][System.Windows.Forms.MessageBox]::Show("���ׂāu�͂��v��I��ł�������
Choose Yes on all dialogs.","Start","OK","Information")

echo "---�v���Z�X��~---"
Stop-Process -Name "VRChatOpenBrowser" 2>&1 > $null

echo "---�t�@�C���폜---"
ls Cert:\CurrentUser\* -Recurse|where Subject -like "CN=*.local.yukiyukivirtual.net"
ls Cert:\CurrentUser\* -Recurse|where Subject -like "CN=*.local.yukiyukivirtual.net"|rm

$UserProperty = $(Get-ItemProperty 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders')
ls "$($UserProperty.Startup)\VRChatOpenBrowser*.lnk"
rm "$($UserProperty.Startup)\VRChatOpenBrowser*.lnk" 
echo "---�n������---"