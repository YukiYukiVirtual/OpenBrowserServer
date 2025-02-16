echo "---インストール開始---"
echo "
---Root証明書を作成---"
$params = @{
    Type = 'Custom'
    Subject = 'CN=Root.local.yukiyukivirtual.net'
    KeySpec = 'Signature'
    KeyExportPolicy = 'Exportable'
    KeyUsage = 'CertSign'
    KeyUsageProperty = 'Sign'
    KeyLength = 2048
    HashAlgorithm = 'sha256'
    NotAfter = (Get-Date).AddYears(10)
    CertStoreLocation = 'Cert:\CurrentUser\My'
}
$rootcert = New-SelfSignedCertificate @params
echo $rootcert

echo "
---サーバー証明書作成---"
$params = @{
    Type = 'Custom'
    Subject = 'CN=Server.local.yukiyukivirtual.net'
    DnsName = 'local.yukiyukivirtual.net'
    KeySpec = 'Signature'
    KeyExportPolicy = 'Exportable'
    KeyLength = 2048
    HashAlgorithm = 'sha256'
    NotAfter = (Get-Date).AddYears(10)
    CertStoreLocation = 'Cert:\CurrentUser\My'
    Signer = $rootcert
    TextExtension = @(
    '2.5.29.37={text}1.3.6.1.5.5.7.3.1')
}
$cert = New-SelfSignedCertificate @params
echo $cert

echo "
---サーバー証明書をファイルに保存して証明書ストアから削除する---"
$MyPwd = ConvertTo-SecureString -String "password" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -Password $MyPwd -FilePath ".\certificate.pfx"
Remove-Item $cert.PSPath

echo "
---Root証明書をRoot証明機関にインストール---"
mv $rootcert.PSPath Cert:\CurrentUser\Root

echo "
---スタートアップにショートカットを作成---"
$UserProperty = $(Get-ItemProperty 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders')
$UserProperty.Startup
$WsShell = New-Object -ComObject WScript.Shell
$Shortcut = $WsShell.CreateShortcut("$($UserProperty.Startup)\VRChatOpenBrowser.lnk")
$Shortcut.TargetPath = "$(pwd)\VRChatOpenBrowser.exe"
$Shortcut.IconLocation = "$(pwd)\VRChatOpenBrowser.exe"
$Shortcut.WorkingDirectory = "$(pwd)"
$Shortcut.Save()

ls Cert:\CurrentUser\* -Recurse|where Subject -like "CN=*.yukiyukivirtual.net"
echo "---インストール完了---"
echo "---VRChatOpenBrowser.exe起動---"
start VRChatOpenBrowser.exe