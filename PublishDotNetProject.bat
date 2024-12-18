@echo off
setlocal

set "DefaultDir=%USERPROFILE%\Desktop\Techolic_Published"

if "%~1"=="" (
    set "PublishDir=%DefaultDir%"
) else (
    set "PublishDir=%~1"
)

echo Publish directory set to: "%PublishDir%"

if not exist "%PublishDir%" (
    mkdir "%PublishDir%"
    if not exist "%PublishDir%" (
        echo Failed to create directory. Please check the path and try again.
        pause
        exit /b 1
    )
)

echo Publishing the project...
dotnet publish -c Release -r win-x64 --self-contained true ^
    /p:TargetFramework=net8.0-windows ^
    /p:PublishSingleFile=true ^
    /p:PublishTrimmed=false ^
    /p:PublishReadyToRun=false ^
    /p:PublishProtocol=FileSystem ^
    /p:PublishDir="%PublishDir%" ^
    /p:IncludeAllContentForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true

set "PublishExitCode=%ERRORLEVEL%"

if %PublishExitCode% EQU 0 (
    echo Publish succeeded! Published files are located at: "%PublishDir%"
) else (
    echo Publish failed with exit code %PublishExitCode%. Please check the error messages above.
)

pause
