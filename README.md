# Put A Pin In It

A small RimWorld mod for marking possible colony landing sites on the **world map** while comparing locations. Right-click a tile and choose **Pin landing site**. Hover over its flag to see the tile details, or left-click the flag to select and highlight its tile. Right-click the flag or tile and choose **Remove landing pin** to unmark it.

![Red pin artwork](Textures/PutAPinInIt/FlagPin.png)

## Workshop screenshots

- [Pin a landing site](docs/screenshots/01-pin-landing-site.jpg)
- [View a flag's tooltip and highlighted hex](docs/screenshots/02-flag-tooltip-and-highlight.jpg)
- [Remove a landing pin](docs/screenshots/03-remove-landing-pin.jpg)

## Scope (v0.1)

- Multiple flags on one generated world, one flag per tile.
- Pins persist in the world's save data via a `WorldComponent`.
- Flags are only visual overlays; they do **not** create world objects, claim tiles, or change whether you can settle.
- Available on the main planet surface during landing-site selection and on the in-game world map.

## Build, test and deploy on Windows

Requires **RimWorld 1.6** with **Harmony** installed, PowerShell, .NET SDK 9.0.200 or later, and the .NET Framework 4.7.2 targeting pack. Open `PutAPinInIt.slnx` in Visual Studio 2022 17.14 or later to work on both projects.

From the repository root, run the following commands as needed.

```powershell
.\scripts\Build-Mod.ps1 -Package
.\scripts\Run-IntegrationTests.ps1
.\scripts\Deploy.ps1
```

The build writes `Assemblies/PutAPinInIt.dll`. With `-Package`, it also writes `dist/PutAPinInIt-0.1.1.zip`, ready to extract into RimWorld's `Mods` folder. Deploy builds Release and copies the runtime files into `Mods/PutAPinInIt`. Use `-SkipBuild` to deploy the existing DLL, or `-ModsDir` to stage the mod elsewhere. Restart RimWorld after updating the DLL, then enable Harmony and Put A Pin In It under **Mods**.

The scripts accept `-RimWorldDir` and `-HarmonyPath` for custom installs. By default, they look for RimWorld in the S: Steam library, then in the standard Steam location, and resolve the Harmony DLL from the same library's Workshop folder. You can also set the `RimWorldDir` and `HarmonyPath` environment variables.

```powershell
.\scripts\Deploy.ps1 -RimWorldDir 'D:\SteamLibrary\steamapps\common\RimWorld' -HarmonyPath 'D:\Mods\Harmony\1.6\Assemblies\0Harmony.dll'
```

Close RimWorld before running the automated tests. The runner deploys the current production build, loads a temporary smoke-test mod, and gives each run its own settings, saves, and logs under `.test-data/`. See the [testing guide](docs/TESTING.md) for the checks and the manual landing-screen walkthrough.

## Credits

Flag artwork: Guy Symonds. Code and documentation developed collaboratively with ChatGPT.

## Licence

See the existing repository `LICENSE` file.
