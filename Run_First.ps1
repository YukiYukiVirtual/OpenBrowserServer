Add-Type -AssemblyName System.Windows.Forms

echo "---注意事項をダイアログに表示---"
[void][System.Windows.Forms.MessageBox]::Show("すべて「はい」を選んでください
Choose Yes on all dialogs.","Start","OK","Information")

echo "
---古いRoot証明書を削除---"
$oldcert = ls Cert:\CurrentUser\Root|where Subject -like "CN=Root.local.yukiyukivirtual.net"
echo $oldcert
if($oldcert){
  rm $oldcert.PSPath
}
else{
  echo "new install"
}

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
---完了---"
[void][System.Windows.Forms.MessageBox]::Show("完了しました","Done","OK","Information")
