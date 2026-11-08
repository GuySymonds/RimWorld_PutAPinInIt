# Put A Pin In It

A small RimWorld mod for marking possible colony landing sites on the **world map** while comparing locations. Select a tile, click **Pin landing site**, and a flag appears on that world tile. Click **Remove landing pin** to unmark it.

![Red pin artwork](Textures/UI/FlagPin.png)

## Scope (v0.1)

- Multiple flags on one generated world, one flag per tile.
- Pins persist in the world's save data via a `WorldComponent`.
- Flags are only visual overlays; they do **not** create world objects, claim tiles, or change whether you can settle.
- Intended for the world/landing selection UI. No extra menus, hotkeys, dependencies or mod settings.

## Installation / build

Requires **RimWorld 1.6** and its managed assemblies. Compile with `dotnet build Source/PutAPinInIt.csproj -p:RimWorldDir="C:/path/to/RimWorld"` (requires the .NET Framework 4.7.2 reference assemblies). The build outputs `Assemblies/PutAPinInIt.dll`.

Copy this directory into `RimWorld/Mods/PutAPinInIt/` and enable it in the Mods menu. The distributed mod needs `About/`, `Assemblies/` and `Textures/`; it does not need `Source/`.

## Status

**Unverified prototype.** The C# has not yet been compiled or exercised against an installed RimWorld 1.6 game. See [test plan](docs/TESTING.md) before publishing. Verify screen-space flag projection, button placement during new-colony landing selection, and API names against installed assemblies.

## Credits

Flag artwork: Guy Symonds. Code and documentation developed collaboratively with ChatGPT.

## Licence

See the existing repository `LICENSE` file.
