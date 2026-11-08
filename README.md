# Put A Pin In It

A small RimWorld mod for marking possible colony landing sites on the **world map** while comparing locations. Select a tile, click **Pin landing site**, and a flag appears on that world tile. Click **Remove landing pin** to unmark it.

![Red pin artwork](Textures/UI/FlagPin.png)

## Scope (v0.1)

- Multiple flags on one generated world, one flag per tile.
- Pins persist in the world's save data via a `WorldComponent`.
- Flags are only visual overlays; they do **not** create world objects, claim tiles, or change whether you can settle.
- Intended for the world/landing selection UI. No extra menus, hotkeys, dependencies or mod settings.

## Build and deploy locally (Windows)

Requires **RimWorld 1.6**, a .NET SDK with .NET Framework 4.7.2 targeting support, and PowerShell. Open `PutAPinInIt.slnx` in a recent Visual Studio (or build it via `dotnet build`). SLNX requires Visual Studio 2022 17.14+ or .NET SDK 9.0.200+.

From the repository root, to build and copy the mod into the default Steam installation:

```powershell
.\scripts\Deploy.ps1
```

If RimWorld lives in a different Steam library, supply its location:

```powershell
.\scripts\Deploy.ps1 -RimWorldDir 'D:\SteamLibrary\steamapps\common\RimWorld'
```

The script builds Release and copies **only** `About/`, `Textures/` and `Assemblies/` into `<RimWorldDir>\Mods\PutAPinInIt`. It does not delete existing files or modify saves. If the Mods folder is elsewhere, use `-ModsDir` to specify it. For an already compiled DLL, use `-SkipBuild`. Build failures prevent deployment.

To build without deploying:

```powershell
dotnet build .\PutAPinInIt.slnx -c Release '-p:RimWorldDir=D:\SteamLibrary\steamapps\common\RimWorld'
```

Restart RimWorld after updating the DLL, then enable the mod under **Mods**.

## Status

**Unverified prototype.** The C# has not yet been compiled or exercised against an installed RimWorld 1.6 game. See [test plan](docs/TESTING.md) before publishing. Verify screen-space flag projection, button placement during new-colony landing selection, and API names against installed assemblies.

## Credits

Flag artwork: Guy Symonds. Code and documentation developed collaboratively with ChatGPT.

## Licence

See the existing repository `LICENSE` file.
