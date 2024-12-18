# Techolics_

An application for generating and managing customized GPOs aligned with CIS benchmarks.

## Local Setup

1. Ensure .NET 8.0 SDK installed.
2. Run `dotnet restore` and `dotnet build`.

## Publishing

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:TargetFramework=net8.0-windows /p:PublishSingleFile=true /p:PublishTrimmed=false /p:PublishReadyToRun=false /p:PublishProtocol=FileSystem /p:PublishDir="%USERPROFILE%\Desktop\TestPublish\" /p:IncludeAllContentForSelfExtract=true /p:EnableCompressionInSingleFile=true
```

App will be published to the specified directory (Desktop/Techolic_Published).