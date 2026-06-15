@echo off
setlocal

cd /d "%~dp0"

echo Building MergeNani (Release, single-file)...
dotnet publish MergeNani\MergeNani.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
if errorlevel 1 exit /b 1

echo.
echo Done: dist\MergeNani.exe
endlocal
