using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PutAPinInIt
{
    // Stored on the World, not the current Map: pins follow the planet through save/load.
    public sealed class WorldPins : WorldComponent
    {
        private List<int> pinnedTiles = new List<int>();
        private HashSet<int> lookup = new HashSet<int>();
        private static Texture2D flag;

        public WorldPins(World world) : base(world) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pinnedTiles, "pinnedTiles", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (pinnedTiles == null) pinnedTiles = new List<int>();
                lookup = new HashSet<int>(pinnedTiles);
            }
        }

        public bool IsPinned(int tile) { return lookup.Contains(tile); }

        public void Toggle(int tile)
        {
            if (tile < 0) return;
            if (lookup.Remove(tile)) pinnedTiles.Remove(tile);
            else if (lookup.Add(tile)) pinnedTiles.Add(tile);
        }

        public override void WorldComponentOnGUI()
        {
            if (Find.World == null || Find.WorldSelector == null) return;

            int selectedTile = Find.WorldSelector.selectedTile;
            if (selectedTile >= 0)
            {
                Rect button = new Rect(UI.screenWidth - 220f, 72f, 200f, 36f);
                if (Widgets.ButtonText(button, IsPinned(selectedTile) ? "Remove landing pin" : "Pin landing site"))
                    Toggle(selectedTile);
            }

            if (Event.current.type != EventType.Repaint) return;
            if (flag == null) flag = ContentFinder<Texture2D>.Get("UI/FlagPin", false);
            if (flag == null) return;

            Camera camera = Find.WorldCamera;
            if (camera == null) return;
            foreach (int tile in pinnedTiles)
            {
                if (tile < 0) continue;
                Vector3 center = Find.WorldGrid.GetTileCenter(tile);
                if (Vector3.Dot(center.normalized, camera.transform.position.normalized) <= 0f) continue;
                Vector3 screen = camera.WorldToScreenPoint(center);
                if (screen.z <= 0f) continue;
                Vector2 ui = new Vector2(screen.x / Prefs.UIScale, UI.screenHeight - screen.y / Prefs.UIScale);
                if (ui.x < 0 || ui.x > UI.screenWidth || ui.y < 0 || ui.y > UI.screenHeight) continue;
                GUI.DrawTexture(new Rect(ui.x - 10f, ui.y - 24f, 24f, 24f), flag, ScaleMode.ScaleToFit, true);
            }
        }
    }
}
