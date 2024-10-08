Add-Type -AssemblyName System.Windows.Forms

echo "---���ӎ������_�C�A���O�ɕ\��---"
[void][System.Windows.Forms.MessageBox]::Show("���ׂāu�͂��v��I��ł�������
Choose Yes on all dialogs.","Start","OK","Information")

echo "
---�Â�Root�ؖ������폜---"
$oldcert = ls Cert:\CurrentUser\Root|where Subject -like "CN=Root.local.yukiyukivirtual.net"
echo $oldcert
if($oldcert){
  rm $oldcert.PSPath
}
else{
  echo "new install"
}

echo "
---Root�ؖ������쐬---"
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
---�T�[�o�[�ؖ����쐬---"
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
---�T�[�o�[�ؖ������t�@�C���ɕۑ����ďؖ����X�g�A����폜����---"
$MyPwd = ConvertTo-SecureString -String "password" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -Password $MyPwd -FilePath ".\certificate.pfx"
Remove-Item $cert.PSPath

echo "
---Root�ؖ�����Root�ؖ��@�ւɃC���X�g�[��---"
mv $rootcert.PSPath Cert:\CurrentUser\Root

echo "
---����---"
[void][System.Windows.Forms.MessageBox]::Show("�������܂���","Done","OK","Information")
