# VRChatOpenBrowser

## ���̃c�[���ɂ���
### VRChat����u���E�U��URL���J��
VRChat�̃��[���h�M�~�b�N�ɂ���āA�f�X�N�g�b�v��̃u���E�U��Web�y�[�W���J�����Ƃ��o���܂��B���C���̋@�\�ɂȂ�܂��B

### VRChat���炱�̃A�v���̏���ǂݍ���
VRChat�̃��[���h�M�~�b�N����StringDownloader���g���āA���̃A�v���̏���JSON�^���Ŏ擾���邱�Ƃ��o���܂��B�A�v�����N��������Ԃ�[http://localhost:21983/](http://localhost:21983/)�ɃA�N�Z�X���Ă��������B

### VRChat����t�@�C����ǂݍ���
VRChat�̃��[���h�M�~�b�N����A���̃A�v�����o�R���ăe�L�X�g�t�@�C���A�摜�t�@�C���A����t�@�C���Ȃǂ��擾���邱�Ƃ��o���܂��B�A�v�����N��������Ԃ�[http://localhost:21983/keys/�t�@�C����](http://localhost:21983/keys/�t�@�C����)�ɃA�N�Z�X���Ă��������B  
�t�@�C����`%appdata%\YukiYukiVirtual\OpenBrowserServer\keys`�ɒu���Ă��������B  
���{����܂ރe�L�X�g�t�@�C�����g�������ꍇ��UTF-16�ɂ����當���������Ȃ��Ǝv���܂��B  
���̋@�\�͂��܂�g��Ȃ��Ǝv���܂��B

## �C���X�g�[��
1. zip�t�@�C�����𓀂��A`setup.exe`�����s���Ă��������B���ڂ̓f�t�H���g�ŃC���X�g�[�����Ă��������B  
2. �ʏ��PC�N�����Ɏ����N�������悤�ɂȂ��Ă��܂��B����N�����E�����ŃA�v�����I���������E�A�v�������������́A�X�^�[�g���j���[����`VRChatOpenBrowser.exe`���N�����Ă��������B
3. VRChat�Őݒ���J���A`�M������Ă��Ȃ�URL������`��L���ɂ��Ă��������B
![](readme_resource/AllowUntrustedUrl.png "VRChat�̐ݒ胁�j���[�̉摜")

## �A�b�v�f�[�g
�ŐV�̃C���X�g�[���[���_�E�����[�h���āA�C���X�g�[�����Ă��������B

## �A���C���X�g�[��
Windows�̐ݒ�A�v������A�v���̃A���C���X�g�[�������s���Ă��������B  
�ݒ�t�@�C���⃍�O�t�@�C����`%appdata%\YukiYukiVirtual\OpenBrowserServer`�ɕۑ����Ă��܂��̂ŁA�K�v�ł���΂�������폜���Ă��������B

## ������@
### �^�X�N�g���C
![](readme_resource/TaskTray.png "�^�X�N�g���C�̉摜")

�^�X�N�g���C�ɐ�̃A�C�R�����\������Ă��܂��B�_�u���N���b�N�ŃR���g���[���p�l�����J���܂��B�E�N���b�N���邱�ƂŎ��s�ł��鍀�ڂ�����܂��B

### �R���g���[���p�l��
![](readme_resource/ControlPanel.png "�R���g���[���p�l���̉摜")

`���h���C���ꗗ`��`���v���g�R���ꗗ`�́A���݋�����Ă�����̂��\������Ă��܂��B**�ݒ�t�@�C��**�̏͂ɏڂ����ڂ��Ă��܂��B
���[�J���̐ݒ�t�@�C����ҏW����`���[�J���ݒ�t�@�C���ǂݍ���`�{�^���������ƈꎞ�I��(�N������)���f����܂��B

VRChat�Ń��[���h�ړ������`�����郏�[���h`�ɔ��f����܂��B

���̑��̃{�^���ɂ��Ă̓}�E�X�J�[�\�����悹��Ɛ������\������܂��B

## �ݒ�t�@�C��
�ݒ�t�@�C���͊�{�I�ɕύX����K�v�͂Ȃ��A�ύX���Ă��A�v���N�����Ɏ����Ń_�E�����[�h����܂��B

�ݒ�t�@�C����`setting.yaml`�Ƃ������O��`%appdata%\YukiYukiVirtual\OpenBrowserServer`�t�H���_�Ƀ_�E�����[�h����܂��B

![](readme_resource/AppData.png "�t�H���_�̉摜")

### YAML�̍���
- Version:
�A�v���̃o�[�W�����ł��B

- IdlePeriod:
�u���E�U���J���v���𕷂��Ԋu(�~���b)�ł��B

- HttpRequestPeriod:
HTTP�v���𕷂��Ԋu(�~���b)�ł��B

- WatchdogTime:
VRChat�������Ă��Ȃ��Ɣ��f���鎞��(��)�ł��B

- Protocol:
�u���E�U���J�����Ƃ��o����URL�̃v���g�R��(�X�L�[��)�ł��B��:https

- Domain:
�u���E�U���J�����Ƃ��o����URL�̃h���C���ł��B�T�u�h���C�����J����悤�ɂȂ�܂��B��:`yukiyukivirtual.net`���w�肷��ƁA`www.yukiyukivirtual.net`���J�����Ƃ��o���܂��B

- BannedUser:
�o�^����Ă��郆�[�U�[�̃��[���h�ɓ������ۂɁA�R���s���[�^�[�̕ی�̂��߂ɂ��̃A�v���̓�����ꎞ��~���܂��B  
���[���h��҂̉��炩�̂�炩���ŁA���̃A�v���ɑ΂��ĕs����������񍐂��������ۂɒǉ�����邱�Ƃ�����܂��B���P�����ƍ폜����邱�Ƃ�����܂��B
  - Id:
BAN���郆�[�U�[��ID�ł��B
  - Reason:
BAN���R�ł��B

## ���O�t�@�C��
���̃A�v���̋N��������̗������c�����e�L�X�g�t�@�C���ł��B�s��񍐂Ȃǂ̎��ɑ����Ă��������B  
���t���Ƃɍ쐬����A�Â��t�@�C������폜����܂��B�ő�10�ێ�����܂��B  
�������ȂǂŊJ�����Ƃ��o���܂��B

## ���[���h�M�~�b�N(���[���h����Ҍ���)
���������܂�

## �Ɛ�
���̃A�v�����g�p���Ĕ������������Ȃ��Q�ɂ��āA����҂͐ӔC�𕉂��܂���B

## �₢���킹
- [Discord](https://discord.gg/9MwqEGvdTm)
- [GitHub](https://github.com/YukiYukiVirtual/OpenBrowserServer)  

�����炩��̂��m�点��Discord�T�[�o�[�ł��܂��B�K�������Ă����Ă��������B  
�s��񍐁A�f�t�H���g�ŋ����Ăق����h���C�����͂����ɕ񍐂��肢���܂��B���ɏd��Ȍ��ׂȂǂ����������ۂ͂����ɂ��m�点���������B  
�C�x���g���ŗ��p����ۂ́A���Ћ����Ă��������I�g�U�ɋ��͂������ł��B
