@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

rem ============================================================
rem  VRChatOpenBrowser release builder
rem
rem  Builds BOTH editions and packs each installer into a zip.
rem  The zips are created next to this .bat file (repository root):
rem    Booth -> VRChatOpenBrowser_v<Version>B.zip
rem    Free  -> VRChatOpenBrowser_v<Version>G.zip
rem
rem  <Version> is read from VersionMaster.txt
rem  (first line that is neither empty nor a "#" comment).
rem
rem  Old VRChatOpenBrowser_*.zip files are deleted at the start,
rem  so only the zips from this run remain.
rem
rem  Each zip contains:
rem    setup.exe, OpenBrowserServer_Installer.msi, README.md, LICENSE.md
rem
rem  Steps per edition:
rem    1. dotnet publish (PublishProfile = edition name)
rem       NOTE: -c <Config> is passed explicitly because the CLI
rem       does NOT apply the Configuration stored in the .pubxml
rem       (it would fall back to Release, which defines BOOTH and
rem       would silently turn the Free edition into a Booth-like
rem       build). Visual Studio applies it, the CLI does not.
rem    2. Build the installer project with devenv.com
rem       (vdproj projects cannot be built by dotnet / MSBuild alone)
rem    3. Zip the files listed above
rem
rem  Old outputs are deleted before each step, so a file that
rem  exists afterwards is guaranteed to be from this run.
rem  Any failure stops the whole run.
rem ============================================================

rem ---- Read version from VersionMaster.txt ----
set "VERSION="
for /f "usebackq tokens=*" %%L in ("VersionMaster.txt") do (
    if not defined VERSION (
        set "LINE=%%L"
        if "!LINE:~0,1!" NEQ "#" set "VERSION=%%L"
    )
)
if not defined VERSION (
    echo [ERROR] Could not read the version from VersionMaster.txt
    goto :Fail
)
echo Version: %VERSION%
echo.

rem ---- Delete old distribution zips ----
if exist "VRChatOpenBrowser_*.zip" (
    echo Deleting old zips: VRChatOpenBrowser_*.zip
    del /f /q "VRChatOpenBrowser_*.zip"
    echo.
)

rem ---- Check dotnet ----
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] dotnet was not found in PATH.
    goto :Fail
)

rem ---- Find devenv.com via vswhere ----
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo [ERROR] vswhere.exe was not found: %VSWHERE%
    goto :Fail
)
set "VSROOT="
for /f "usebackq tokens=*" %%D in (`"%VSWHERE%" -latest -property installationPath`) do set "VSROOT=%%D"
if not defined VSROOT (
    echo [ERROR] Visual Studio installation was not found by vswhere.
    goto :Fail
)
set "DEVENV=%VSROOT%\Common7\IDE\devenv.com"
if not exist "%DEVENV%" (
    echo [ERROR] devenv.com was not found: %DEVENV%
    goto :Fail
)
echo Visual Studio: %VSROOT%
echo.

rem ---- Build both editions ----
call :BuildOne Booth B
if errorlevel 1 goto :Fail
call :BuildOne Free G
if errorlevel 1 goto :Fail

echo.
echo ============================================================
echo  All done:
echo    VRChatOpenBrowser_v%VERSION%B.zip
echo    VRChatOpenBrowser_v%VERSION%G.zip
echo ============================================================
pause
exit /b 0

:Fail
echo.
echo [FAILED] Release build stopped. See the messages above.
pause
exit /b 1


rem ============================================================
rem  :BuildOne <Config> <Suffix>
rem    Config: Booth / Free    Suffix: B / G
rem ============================================================
:BuildOne
set "CONFIG=%~1"
set "SUFFIX=%~2"
set "OUTDIR=OpenBrowserServer_Installer\%CONFIG%"
set "ZIPNAME=VRChatOpenBrowser_v%VERSION%%SUFFIX%.zip"
set "ZIPPATH=%ZIPNAME%"

echo ------------------------------------------------------------
echo  [%CONFIG%] Step 1/3: publish
echo ------------------------------------------------------------
if exist "publish\VRChatOpenBrowser.exe" del /f /q "publish\VRChatOpenBrowser.exe"
dotnet publish "OpenBrowserServer\OpenBrowserServer.csproj" -c %CONFIG% /p:PublishProfile=%CONFIG% --nologo
if errorlevel 1 (
    echo [ERROR] dotnet publish failed: %CONFIG%
    exit /b 1
)
if not exist "publish\VRChatOpenBrowser.exe" (
    echo [ERROR] Published exe was not produced: publish\VRChatOpenBrowser.exe
    exit /b 1
)

echo.
echo ------------------------------------------------------------
echo  [%CONFIG%] Step 2/3: build installer
echo ------------------------------------------------------------
if exist "%OUTDIR%\setup.exe" del /f /q "%OUTDIR%\setup.exe"
if exist "%OUTDIR%\OpenBrowserServer_Installer.msi" del /f /q "%OUTDIR%\OpenBrowserServer_Installer.msi"
"%DEVENV%" "OpenBrowserServer.sln" /Build "%CONFIG%|Any CPU" /Project "OpenBrowserServer_Installer"
if errorlevel 1 (
    echo [ERROR] Installer build failed: %CONFIG%
    exit /b 1
)
if not exist "%OUTDIR%\setup.exe" (
    echo [ERROR] setup.exe was not produced: %OUTDIR%\setup.exe
    exit /b 1
)
if not exist "%OUTDIR%\OpenBrowserServer_Installer.msi" (
    echo [ERROR] Installer .msi was not produced: %OUTDIR%\OpenBrowserServer_Installer.msi
    exit /b 1
)

echo.
echo ------------------------------------------------------------
echo  [%CONFIG%] Step 3/3: create zip
echo ------------------------------------------------------------
if exist "%ZIPPATH%" del /f /q "%ZIPPATH%"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -LiteralPath '%OUTDIR%\setup.exe','%OUTDIR%\OpenBrowserServer_Installer.msi','README.md','LICENSE.md' -DestinationPath '%ZIPPATH%' -Force"
if errorlevel 1 (
    echo [ERROR] Zip creation failed: %CONFIG%
    exit /b 1
)
if not exist "%ZIPPATH%" (
    echo [ERROR] Zip was not produced: %ZIPPATH%
    exit /b 1
)
echo [OK] %ZIPPATH%
echo.
exit /b 0
