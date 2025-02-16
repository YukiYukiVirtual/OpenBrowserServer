Add-Type -AssemblyName System.Windows.Forms

echo "---始末開始---"
[void][System.Windows.Forms.MessageBox]::Show("すべて「はい」を選んでください
Choose Yes on all dialogs.","Start","OK","Information")

echo "---プロセス停止---"
Stop-Process -Name "VRChatOpenBrowser" 2>&1 > $null

echo "---ファイル削除---"
ls Cert:\CurrentUser\* -Recurse|where Subject -like "CN=*.local.yukiyukivirtual.net"
ls Cert:\CurrentUser\* -Recurse|where Subject -like "CN=*.local.yukiyukivirtual.net"|rm

$UserProperty = $(Get-ItemProperty 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders')
ls "$($UserProperty.Startup)\VRChatOpenBrowser*.lnk"
rm "$($UserProperty.Startup)\VRChatOpenBrowser*.lnk" 
echo "---始末完了---"