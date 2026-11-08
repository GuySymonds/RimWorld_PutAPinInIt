using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PutAPinInIt
{
    public sealed class WorldPinsMod : Mod
    {
        public WorldPinsMod(ModContentPack content) : base(content)
        {
            new Harmony("GuySymonds.PutAPinInIt").PatchAll(typeof(WorldPinsMod).Assembly);
        }
    }

    // This callback runs both on the landing-site page and on the in-game world map.
    [HarmonyPatch]
    internal static class WorldPinsUI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WorldInterface), nameof(WorldInterface.WorldInterfaceOnGUI))]
        private static void DrawFlags(WorldInterface __instance)
        {
            if (CanShowPins(__instance))
                Find.World.GetComponent<WorldPins>()?.DrawFlags();
        }

        // Vanilla handles windows first, then calls this method for map clicks.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WorldInterface), nameof(WorldInterface.HandleLowPriorityInput))]
        private static void HandlePinClicks(WorldInterface __instance)
        {
            var input = Event.current;
            if (input.type != EventType.MouseDown || (input.button != 0 && input.button != 1) ||
                !CanShowPins(__instance) || Mouse.IsInputBlockedNow ||
                __instance.targeter.IsTargeting || __instance.tilePicker.Active || __instance.routePlanner.Active)
                return;

            var pins = Find.World.GetComponent<WorldPins>();
            if (pins == null) return;

            var selector = __instance.selector;
            if (input.button == 1 && selector.SelectedObjects.Exists(obj => obj is Caravan caravan && caravan.IsPlayerControlled))
                return;

            PlanetTile flagTile = pins.FlagUnderMouse();
            if (input.button == 0)
            {
                if (!flagTile.Valid) return;
                selector.ClearSelection();
                selector.SelectedTile = flagTile;
            }
            else
            {
                var option = pins.PinOption(flagTile.Valid ? flagTile : GenWorld.MouseTile());
                if (option == null) return;
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption> { option }));
            }

            input.Use();
        }

        private static bool CanShowPins(WorldInterface worldUI)
        {
            return WorldRendererUtility.WorldSelected &&
                Find.WorldGrid.Surface != null &&
                worldUI.selector.SelectedLayer == Find.WorldGrid.Surface &&
                Find.WorldCamera != null &&
                Find.WorldCamera.gameObject.activeInHierarchy &&
                !Find.UIRoot.screenshotMode.FiltersCurrentEvent;
        }
    }
}
