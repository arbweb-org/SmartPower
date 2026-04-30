@echo off
setlocal

REM ===== CONFIG =====
set PROJECT_PATH=D:\Projects\Github\arbweb-org\SmartPower\SmartPower.Publisher
set RUNTIME=linux-arm64
set CONFIG=Release
set TARGET=root@192.168.1.182
set REMOTE_PATH=/opt/smartpower
set SERVICE=smartpower.service
set BINARY=SmartPower.Publisher

REM ===== BUILD & PUBLISH =====
echo Publishing...
dotnet publish "%PROJECT_PATH%" -c %CONFIG% -r %RUNTIME% --self-contained true -p:PublishSingleFile=true

REM ===== FIND PUBLISH FOLDER =====
set PUBLISH_DIR=%PROJECT_PATH%\bin\%CONFIG%\net10.0\%RUNTIME%\publish

echo Publishing folder: %PUBLISH_DIR%

REM ===== STOP SERVICE =====
echo Stopping service...
ssh %TARGET% "sudo systemctl stop %SERVICE%"

REM ===== CLEAN REMOTE FOLDER =====
echo Cleaning remote folder...
ssh %TARGET% "sudo find %REMOTE_PATH% -mindepth 1 -delete"

REM ===== COPY FILES =====
echo Copying files...
scp -O -r "%PUBLISH_DIR%\*" %TARGET%:%REMOTE_PATH%

REM ===== SET EXECUTE PERMISSION =====
echo Setting executable permission...
ssh %TARGET% "sudo chmod +x %REMOTE_PATH%/%BINARY%"

REM ===== REBOOT =====
echo Rebooting Raspberry Pi...
ssh %TARGET% "sudo reboot"

echo Done.
pause
endlocal