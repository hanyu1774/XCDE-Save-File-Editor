# XCDE Save File Editor
<img width="1885" height="1043" alt="xcde_save_file_editor_user_interface" src="https://github.com/user-attachments/assets/4a746062-fe43-4972-9d04-083de60c72de" />

This application is intended to edit your save file from Xenoblade Chronicles Definitive Edition. 

## Current Status
**This repo is a work of progress! It isn't entirely finished!**

**This application was not fully tested yet. No safety guarantees are made! This application is shipped as is! I am not responsible
for any damages nor any safe file corruption!** With that being said... This application does its job editing a save file and save the results in a save file. 
As things currently stand: An edited save file are correctly loaded by the game.

**Currently:** When you use this application, you will see some confusing input fields and various things (e.g. gemstones, crystals, arts, etc.) have no names, while other things have display names (armor, weapons, etc.).

## Needed dependencies
You only need the .NET 10 SDK. For x64 systems. Downnload it and install. If you are on Linux, then the .NET 10 SDK can be installed via the distro's (Debian / Ubuntu? => `apt`. Arch Linux? => `pacman`) package manager.

## How to use it
**Please read the 'Needed dependencies' section first!**
Currently, there is no executable binary published. You can however run the code by using the following commands. Then you can use the application.

With a terminal, navigate to this project folder and run...

```shell
dotnet build
```

Afterwards, run...
```shell
dotnet run --project XCDESaveEditor.Gui 
```

This will launch the XCDE Save File Editor

## Work that needs to be done...

* Get more string names for objects
* Optimize the UI for more intiutive usage
* Remove obsolete things
* More analyzation of a save file is required

## Documentation
This section 
