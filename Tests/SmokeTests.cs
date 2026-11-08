using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PutAPinInIt.Tests
{
    public sealed class SmokeTests : GameComponent
    {
        private const string SaveName = "PutAPinInItSmoke";
        private static readonly List<string> results = new List<string>();
        private static bool awaitingReload;
        private static int keptTile;
        private static int removedTile;
        private bool ran;

        public SmokeTests(Game game) { }

        public override void GameComponentUpdate()
        {
            if (ran || !GenCommandLine.CommandLineArgPassed("putapinit-tests") ||
                Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null ||
                LongEventHandler.AnyEventNowOrWaiting)
                return;

            ran = true;
            try
            {
                var surface = Find.WorldGrid.Surface;
                var pins = Find.World.GetComponent<WorldPins>();
                Require(pins != null, "World component missing");

                if (awaitingReload)
                {
                    Require(pins.IsPinned(new PlanetTile(keptTile, surface)), "Kept pin missing after reload");
                    Require(!pins.IsPinned(new PlanetTile(removedTile, surface)), "Removed pin reappeared");
                    results.Add("PASS Real save and reload preserve the remaining pin");
                    Finish(0);
                    return;
                }

                var first = Find.CurrentMap.Tile;
                var second = new PlanetTile((first.tileId + 1) % surface.TilesCount, surface);
                Require(!pins.IsPinned(first) && !pins.IsPinned(second), "New world started with pins");
                results.Add("PASS Generated world starts without pins");

                pins.Toggle(PlanetTile.Invalid);
                var outOfBounds = new PlanetTile(surface.TilesCount, surface);
                pins.Toggle(outOfBounds);

                pins.Toggle(first);
                pins.Toggle(second);
                Require(pins.IsPinned(first) && pins.IsPinned(second), "Multiple pins did not coexist");
                pins.Toggle(first);
                Require(!pins.IsPinned(first) && pins.IsPinned(second), "Removing one pin affected the wrong tile");
                results.Add("PASS Two pins coexist and can be removed independently");

                foreach (string method in new[] { nameof(WorldInterface.WorldInterfaceOnGUI), nameof(WorldInterface.HandleLowPriorityInput) })
                {
                    var patch = Harmony.GetPatchInfo(AccessTools.Method(typeof(WorldInterface), method));
                    Require(patch != null && patch.Owners.Contains("GuySymonds.PutAPinInIt"), $"World UI hook missing from {method}");
                }
                var flag = WorldPins.Flag;
                Require(flag != null && flag != BaseContent.BadTex && flag.width == 128 && flag.height == 128,
                    "RimWorld did not decode the flag artwork");
                results.Add("PASS World UI hooks and decoded flag artwork are loaded");

                Find.WorldSelector.SelectedTile = first;
                string details = pins.TileDetails(second);
                Vector2 coordinates = surface.LongLatOf(second);
                Require(details.Contains($"{coordinates.y.ToStringLatitude()} {coordinates.x.ToStringLongitude()}") &&
                    details.Contains(GenTemperature.GetAverageTemperatureLabel(second)) &&
                    Find.WorldSelector.SelectedTile == first, "Tooltip did not describe the pinned tile independently of selection");
                results.Add("PASS Tooltip describes the pinned tile without changing selection");

                keptTile = second.tileId;
                removedTile = first.tileId;
                GameDataSaveLoader.SaveGame(SaveName);
                var save = new XmlDocument();
                save.Load(Path.Combine(GenFilePaths.SavedGamesFolderPath, SaveName + ".rws"));
                var savedPins = save.SelectNodes("//li[@Class='PutAPinInIt.WorldPins']/pinnedTiles/li");
                Require(savedPins.Count == 1 && savedPins[0].InnerText == keptTile.ToString(),
                    "Save should contain only the kept pin");
                results.Add("PASS Save excludes invalid, out-of-range, and removed tiles");
                awaitingReload = true;
                GameDataSaveLoader.LoadGame(SaveName);
            }
            catch (Exception exception)
            {
                results.Add("FAIL " + exception);
                Finish(1);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Finish(int exitCode)
        {
            File.WriteAllLines(Path.Combine(GenFilePaths.SaveDataFolderPath, "Results.txt"), results);
            Application.Quit(exitCode);
        }
    }
}
