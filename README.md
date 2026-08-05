# XCDE Save File Editor
<img width="1885" height="1043" alt="xcde_save_file_editor_user_interface" src="https://github.com/user-attachments/assets/4a746062-fe43-4972-9d04-083de60c72de" />

This application is intended to edit your save file from Xenoblade Chronicles Definitive Edition. 

***Does it work on Linux?*** Yes, it does!

## Current Status
**This repo is a work of progress! It isn't entirely finished!**

**This application was not fully tested yet. No safety guarantees are made! This application is shipped as is! I am not responsible
for any damages nor any safe file corruption!** With that being said... This application does its job editing a save file and save the results in a save file. 
As things currently stand: An edited save file are correctly loaded by the game.

**Currently:** When you use this application, you will see some confusing input fields and various things (e.g. gemstones, crystals, arts, etc.) have no names, while other things have display names (armor, weapons, etc.).

## Needed dependencies
You only need the .NET 10 SDK. For x64 systems. Downnload it and install. If you are on Linux, then the .NET 10 SDK can be installed via the distro's (Debian / Ubuntu? => `apt`. Arch Linux? => `pacman`) package manager.
After installation, you can use the .NET 10 SDK for C# development and you can use the `dotnet` command for CLI purposes in a terminal/console.

### Optional step: disable telemetry
By default, once you have the .NET SDK installed, telemetry is enabled. It can be disabled, either only for the current session or permanently. I will show you how to permanently disabled it.

#### Windows
Run the PowerShell terminal (in modern Windows 11 installations, the PowerShell profile is default once you start '(Windows) Terminal') and run the following command for your own user account:

```powershell
[Environment]::SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1", "User")
```

If you want to disable it system-wide:
```powershell
[Environment]::SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1", "Machine")
```
After executing this system-wide setting: setting only works in your next terminal/console session. So either close your terminal/console after executing said command, then start a new session. Or run this command afterwards within the same terminal/console session:
```powershell
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
```

#### Linux
On Linux, if you use the Fish Shell: 

```fish
set -Ux DOTNET_CLI_TELEMETRY_OPTOUT 1
```

If you use the Bash shell:

```bash
echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1' >> ~/.bashrc
```

## How to use it
### Important notice
**Please read the 'Needed dependencies' section first!**
**Since certain things lack display names, you need to check out those two web pages, which have information about the BDAT tables in XCDE!**
* https://xenobladedata.github.io/
* https://xenoblade.github.io/xb1/bdat/index.html

The game makes heavy use of what's called "BDAT" tables. This is where all "blueprints" and definitions for items are. The second link shows you a list of BDAT tables within `bdat_common`. Here, you can find for example item-related tables. 

Why looking up BDAT tables? Since various things lack display names, you can use the IDs for certain items, if you want to add them to your save file.

### How to run the project
Currently, there is no executable binary published. You can however run the code by using the following commands. Then you can use the application.

With a terminal/console, navigate to this project folder using `cd "path/to/project/"`. You need to be in the root of the project folder. Then run...

```shell
dotnet build
```

Afterwards, run...
```shell
dotnet run --project XCDESaveEditor.Gui 
```

This will launch the XCDE Save File Editor.

## Work that needs to be done...

* Get more string names for objects
* Optimize the UI for more intiutive usage
* Remove obsolete things
* More analyzation of a save file is required

## Documentation
### Origin

This project builds on top of [XCDESave](https://gitlab.com/damysteryman/XCDESave), a library written by `damysteryman` that reverse engineers and parses the save file format used by Xenoblade Chronicles: Definitive Edition (bfsgame*.sav files). `XCDESave` is licensed under the GNU General Public License v3.0, and its structure documentation (offsets, field meanings) came from community reverse engineering work, some of it still incomplete or unconfirmed at the time of writing.

Xenoblade Chronicles: Definitive Edition runs on the Xenoblade Chronicles 2 engine rather than a modernized version of the original Wii engine. That is why its data uses the BDAT database format that Monolith Soft introduced with Xenoblade Chronicles 2, and why old Wii era tools and cheat codes for the original release do not carry over directly.

On top of XCDESave, this repository adds `XCDESaveEditor.Gui`, a desktop application that gives the library an actual interface, wraps it in a proper editing workflow, adds validation, and fills in item, weapon, armor, gem, arts and skill names that are not present in XCDESave itself.

### What it does

This application lets you open a save file, edit it through a graphical interface, and write the changes back:

* Party and characters: add or remove characters from the active party, edit level, AP, and arts for any of the 15 playable characters, plus an advanced view of still unidentified per character data blocks
* Weapons and the five armor slots (head, torso, arm, leg, foot): add, edit, or remove equipped items, including items that are not normally obtainable in game, with gem socket editing
* Gemstones and crystals: add, edit, or remove items in both boxes, with resolved display names instead of raw numeric IDs
* Other items: collectables, materials, key items, and arts manuals, including quantity editing
* Money and Noponstones

A backup copy of the save file is created automatically the first time it is loaded.

### How it is built
Photino.NET provides the desktop window and the native OS webview (WebView2, WebKitGTK, or WKWebView depending on platform), so the interface itself is plain HTML, CSS, and JavaScript with no separate build step.

The backend follows Flow Design, a software design methodology built around small, single purpose Models, Flows, and Workflows. It is used for the main logic of this application. 

Item, weapon, armor, gem, crystal, arts, and skill names come from community documented BDAT table dumps of the game's internal databases, cross referenced against real save files to confirm the ID scheme and to disambiguate items that would otherwise share the same display name.

Because `XCDESave` is GPL-3.0 licensed and this project depends on it directly, distributing `XCDESaveEditor.Gui` as a combined work is subject to the same license.

### What ist Photino.NET?
It's basically a much more efficient and modern solution for building desktop apps. 

In .NET when building desktop, you probably heard of WPF, WinForms, MAUI etc. as solutions for building desktop apps. Photino.NET isn't plagued by the XAML nonsense and dependency on outdated stuff from more than 10 years ago unlike WPF, WinForms and MAUI. Secondly, Photino.NET is much more resource-efficient and easier to use than the other alternatives. 

