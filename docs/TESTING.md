# Testing Put A Pin In It

Run `./scripts/Run-IntegrationTests.ps1` with RimWorld closed and Harmony installed. It builds both projects, deploys the mod, and launches a new RimWorld game with its own settings and saves. Each run prints the path to its `Results.txt`. Check `Player.log` in the same folder if startup or a test fails. The smoke-test mod is temporary and the release ZIP contains only the production mod files and licence.

The smoke test checks that a generated world starts with no pins, pins can be added and removed independently, and the UI patches and flag artwork load. It checks that tooltip text describes the pinned tile while a different tile is selected. It then inspects and reloads a real save to confirm that only the retained pin survives.

## Manual landing-screen check

1. Enable Harmony and Put A Pin In It. Start a new colony and generate a world.
2. Right-click an empty surface tile and choose **Pin landing site**. Check that the red flag appears during initial landing-site selection.
3. Pin two separated tiles. Rotate and zoom the globe. Check that flags track their tiles, disappear behind the globe, and stay legible at your UI scale.
4. Hover over each flag and check that its tooltip describes its own tile. Left-click a flag and check that the correct hex and terrain pane become selected. Right-click the first flag and choose **Remove landing pin**. Check that the second pin remains.
5. Settle on the remaining pinned tile if it is eligible. Open the world map, check the pin, save, and reload. Check that the pin still appears and that the pin controls are absent from the local colony map.
6. Generate another world and check that it has no pins from the first world. If you use Odyssey, switch to the orbit layer and check that surface pins and controls are hidden.
7. With a player caravan selected, check that right-click still issues the vanilla caravan order. Check `Player.log` for exceptions during the walkthrough.
