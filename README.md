# Techolics_ - SIH 2024 Project

An application for generating and managing customized GPOs aligned with CIS benchmarks.

## Team Name

Techolics_

## Team Members

1. [Shravanpuri Goswami](https://github.com/shravanngoswamii) - Team Leader
2. [Jash Ambaliya](https://github.com/AJ0070)
3. [Jitendra Verma](https://github.com/jitendravjh/)
4. [Niyati Rajput](https://github.com/Niyaaatii)
5. [Pankaj Kumar Bind](https://github.com/pankaj-bind)
6. [Ramlakhan Madheshiya](https://github.com/ramlakhanmadheshiya)

### PS ID

SIH1686

### Problem Statement Title

Tools and techniques for customisation of GPO as per CIS guidelines to deploy on offline / standalone windows.

### Description

Background: Group Policy Objects (GPOs) are powerful tools in Windows environments, used to centrally manage and enforce system settings, security configurations, and user preferences across a network of computers. The Center for Internet Security (CIS) provides detailed guidelines and benchmarks for securing various operating systems, including Windows 10 and 11. Implementing CIS benchmarks through customized GPOs ensures that systems adhere to industry best hardening practices, reducing vulnerabilities and enhancing overall security posture.

#### Detailed Description
1. Deploying customized GPOs, based on CIS guidelines or user requirements is essential for hardening Windows systems. It will help in ensuring robust security and maintaining compliance with industry standards.  
2. The guidelines contain multiple system configurations in terms of registry settings and group policy settings. Deploying of these settings as per user requirement is a daunting task, considering the availability of limited tools and human-intensive efforts.  
3. Present problem statement is an attempt to explore the possible tools and techniques to automate the task of generating and managing the GPOs as per user requirements for various types of systems including airgapped/standalone machines.

#### Expected Solution:
Following functionalities have been envisaged for the expected solution:
- [x] (a) To create, edit and manage GPOs as per CIS guidelines and user requirements, if needed.
- [x] (b) The customised GPOs generated should be deployable on airgapped / standalone system.
- [x] (c) To maintain multiple group / category of system hardening settings with appropriate documentation to define the configuration details for each of the group / category.
- [x] (d) Import / Export of configuration details for catering to user requirement to maintain multiple group / category of system configurations.
- [x] (e) Tool should be able to import documentation from CIS guidelines available in PDF format.
- [x] (f) Envisaged tool should support in deploying GPOs on the target machine.
- [x] (g) Envisaged tool should support in testing and auditing of system configurations on the target machine.


## Local Setup

1. Ensure .NET 8.0 SDK installed.
2. Run `dotnet restore` and `dotnet build`.

## Publishing

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:TargetFramework=net8.0-windows /p:PublishSingleFile=true /p:PublishTrimmed=false /p:PublishReadyToRun=false /p:PublishProtocol=FileSystem /p:PublishDir="%USERPROFILE%/Desktop/TestPublish/" /p:IncludeAllContentForSelfExtract=true /p:EnableCompressionInSingleFile=trues
```

App will be published to the specified directory (Desktop/Techolic_Published).

## Publish Profile

Use this Publish Profile for GUI based publishing.

Save this as `Standalone.pubxml` in the project directory (`Techolics_\Properties\PublishProfiles\Standalone.pubxml`).

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>false</PublishTrimmed>
    <PublishReadyToRun>false</PublishReadyToRun>
    <PublishProtocol>FileSystem</PublishProtocol>
    <Configuration>Release</Configuration>
    <PublishDir>$(UserProfile)\Desktop\TestPublish\</PublishDir>
    <IncludeAllContentForSelfExtract>true</IncludeAllContentForSelfExtract>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
  </PropertyGroup>
</Project>
```
