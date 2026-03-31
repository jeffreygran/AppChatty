# AppChatty

A Windows-based Copilot-style assistant that lives in your system tray and
surfaces the right **M365 Copilot agent** for whichever legacy application you
are currently using.

## Features

| Feature | Details |
|---|---|
| **System tray** | Sits quietly in the notification area; left-click or double-click to toggle |
| **Right-side panel** | Borderless, always-on-top form docked to the right edge of your screen |
| **Active-window detection** | Polls `GetForegroundWindow` (Win32) every 2 seconds |
| **Agent routing** | Maps the detected process name to the correct M365 Copilot agent URL |
| **WebView2 shell** | Embeds the M365 Copilot chat experience directly in the panel |

## Supported Applications → Agents

| Application | Process match | M365 Copilot agent |
|---|---|---|
| Microsoft Excel | `excel` | `?app=excel` |
| Microsoft Word | `winword` / `word` | `?app=word` |
| Microsoft PowerPoint | `powerpnt` / `powerpoint` | `?app=powerpoint` |
| Microsoft Outlook | `outlook` | `?app=outlook` |
| Microsoft Teams | `teams` | `?app=teams` |
| Microsoft OneNote | `onenote` | `?app=onenote` |
| SharePoint | `sharepoint` | `?app=sharepoint` |
| Microsoft Dynamics | `dynamics` | `?app=dynamics365` |
| SAP | `sap` | `?app=sap` |
| Oracle | `oracle` | `?app=oracle` |
| Salesforce | `salesforce` | `?app=salesforce` |
| *(any other app)* | — | Generic M365 Copilot chat |

## Prerequisites

| Requirement | Version |
|---|---|
| Windows | 10 (1803) or later |
| .NET Framework | 4.8 |
| WebView2 Runtime | [Download](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) |
| Visual Studio | 2022 (or MSBuild 17+) |

## Build

```
git clone https://github.com/jeffreygran/AppChatty.git
cd AppChatty
dotnet restore AppChatty.sln
dotnet build   AppChatty.sln --configuration Release
```

## Run

Double-click `bin\Release\AppChatty.exe` — or run it from the command line:

```
src\AppChatty\bin\Release\AppChatty.exe
```

The application starts minimised to the system tray.  Click the tray icon to
open the Copilot panel on the right side of your screen.

## Test

```
dotnet test AppChatty.sln
```

## Project Structure

```
AppChatty.sln
src/
  AppChatty.Core/
    AgentResolver.cs            App-name → M365 agent URL mapping
    AppChatty.Core.csproj       .NET Standard 2.0 (shared logic)
  AppChatty/
    Program.cs                  Entry point, single-instance Mutex
    TrayApplicationContext.cs   NotifyIcon + lifecycle management
    CopilotPanel.cs             Side-panel Form (code-behind)
    CopilotPanel.Designer.cs    Designer-generated layout
    ActiveWindowWatcher.cs      Win32 foreground-window polling
    Resources/
      icon.ico                  Tray / window icon
    AppChatty.csproj            .NET Framework 4.8, WinForms + WebView2
  AppChatty.Tests/
    AgentResolverTests.cs       MSTest unit tests (pure logic)
    AppChatty.Tests.csproj      net8.0 (runs cross-platform)
```
