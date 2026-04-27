@echo off
setlocal

REM ===== CONFIG =====
set PROJECT_PATH=D:\Projects\Github\arbweb-org\SmartPower\SmartPower.Publisher
set RUNTIME=linux-arm64
set CONFIG=Release
set TARGET=root@192.168.1.182:/opt/smartpower

REM ===== BUILD & PUBLISH =====
echo Publishing...
dotnet publish "%PROJECT_PATH%" -c %CONFIG% -r %RUNTIME% --self-contained true -p:PublishSingleFile=true

REM ===== FIND PUBLISH FOLDER =====
set PUBLISH_DIR=%PROJECT_PATH%\bin\%CONFIG%\net10.0\%RUNTIME%\publish

echo Publishing folder: %PUBLISH_DIR%

REM ===== COPY TO SERVER =====
echo Copying to Raspberry Pi...
scp -O -r "%PUBLISH_DIR%\*" %TARGET%

echo Done.
pause
endlocal