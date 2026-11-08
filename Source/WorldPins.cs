using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PutAPinInIt
{
    // Stored on the World, not the current Map: pins follow the planet through save/load.
    [StaticConstructorOnStartup]
    public sealed class WorldPins : WorldComponent
    {
        internal static readonly Texture2D Flag = ContentFinder<Texture2D>.Get("PutAPinInIt/FlagPin");
        private List<int> pinnedTiles = new List<int>();

        public WorldPins(World world) : base(world) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pinnedTiles, "pinnedTiles", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pinnedTiles == null)
                pinnedTiles = new List<int>();
        }

        public bool IsPinned(PlanetTile tile)
        {
            return IsSurfaceTile(tile) && pinnedTiles.Contains(tile.tileId);
        }

        public void Toggle(PlanetTile tile)
        {
            if (!IsSurfaceTile(tile)) return;
            if (!pinnedTiles.Remove(tile.tileId)) pinnedTiles.Add(tile.tileId);
        }

        internal void DrawFlags()
        {
            if (Event.current.type != EventType.Repaint || Flag == null || Flag == BaseContent.BadTex) return;

            int hoveredTile = -1;
            Rect hoverRect = default;
            foreach (int tile in pinnedTiles)
            {
                if (!TryGetFlagRect(tile, out Rect rect)) continue;
                GUI.DrawTexture(rect, Flag, ScaleMode.ScaleToFit, true);
                Rect hitRect = FlagHitRect(rect);
                if (Mouse.IsOver(hitRect))
                {
                    hoveredTile = tile;
                    hoverRect = hitRect;
                }
            }

            if (hoveredTile >= 0)
            {
                var tile = new PlanetTile(hoveredTile, world.grid.Surface);
                TooltipHandler.TipRegion(hoverRect, () => TileDetails(tile), Gen.HashCombineInt(480137, hoveredTile));
            }
        }

        internal PlanetTile FlagUnderMouse()
        {
            if (Flag == null || Flag == BaseContent.BadTex) return PlanetTile.Invalid;
            for (int i = pinnedTiles.Count - 1; i >= 0; i--)
            {
                int tile = pinnedTiles[i];
                if (TryGetFlagRect(tile, out Rect rect) && Mouse.IsOver(FlagHitRect(rect)))
                    return new PlanetTile(tile, world.grid.Surface);
            }
            return PlanetTile.Invalid;
        }

        private bool TryGetFlagRect(int tile, out Rect rect)
        {
            rect = default;
            var surface = world.grid.Surface;
            if (tile < 0 || tile >= surface.TilesCount) return false;
            Camera camera = Find.WorldCamera;
            Vector3 radial = surface.GetTileCenter(tile);
            Vector3 center = surface.Origin + radial;
            Vector3 toCamera = camera.transform.position - center;
            if (Vector3.Dot(radial, toCamera) <= 0f || Vector3.Dot(camera.transform.forward, toCamera) >= 0f)
                return false;
            Vector2 ui = GenWorldUI.WorldToUIPosition(center);
            if (ui.x < 0 || ui.x > UI.screenWidth || ui.y < 0 || ui.y > UI.screenHeight) return false;

            // Anchor the foot of the pole to the tile, accounting for transparent padding.
            rect = new Rect(ui.x - 5f, ui.y - 22f, 32f, 32f);
            return true;
        }

        // Limit hit testing to the artwork inside the padded texture.
        private static Rect FlagHitRect(Rect drawRect)
        {
            return new Rect(drawRect.x + 3f, drawRect.y, 19f, 23f);
        }

        internal string TileDetails(PlanetTile tile)
        {
            var terrain = tile.Tile;
            Vector2 coordinates = world.grid.LongLatOf(tile);
            var details = new StringBuilder();
            details.AppendLine(terrain.PrimaryBiome.LabelCap);
            details.AppendLine($"{coordinates.y.ToStringLatitude()} {coordinates.x.ToStringLongitude()}");
            if (terrain.HillinessLabel != Hilliness.Undefined)
                details.AppendLine($"{"Terrain".Translate()}: {terrain.HillinessLabel.GetLabelCap()}");
            if (terrain.PrimaryBiome.canBuildBase)
                details.AppendLine($"{"StoneTypesHere".Translate()}: {world.NaturalRockTypesIn(tile).Select(rock => rock.label).ToCommaList(useAnd: true).CapitalizeFirst()}");
            details.AppendLine($"{"Elevation".Translate()}: {terrain.Layer.Def.elevationString.Formatted(terrain.elevation.ToString("F0"))}");
            details.AppendLine($"{"AvgTemp".Translate()}: {GenTemperature.GetAverageTemperatureLabel(tile)}");
            details.AppendLine($"{"OutdoorGrowingPeriod".Translate()}: {Zone_Growing.GrowingQuadrumsDescription(tile)}");
            details.Append($"{"Rainfall".Translate()}: {terrain.rainfall:F0}mm");
            if (ModsConfig.BiotechActive)
                details.Append($"\n{"TilePollution".Translate()}: {terrain.pollution.ToStringPercent()}");
            return details.ToString();
        }

        internal FloatMenuOption PinOption(PlanetTile tile)
        {
            if (!IsSurfaceTile(tile)) return null;
            string label = IsPinned(tile) ? "Remove landing pin" : "Pin landing site";
            return new FloatMenuOption(label, () => Toggle(tile));
        }

        private bool IsSurfaceTile(PlanetTile tile)
        {
            var surface = world.grid.Surface;
            return tile.Valid && surface != null && tile.Layer == surface && tile.tileId < surface.TilesCount;
        }
    }
}
