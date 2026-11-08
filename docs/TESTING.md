# Verification checklist

This source scaffold has **not** been compiled or tested in game. Before release:

1. Build against a locally installed **RimWorld 1.6**; resolve API/reference mismatches instead of adding speculative compatibility shims.
2. Generate a new world. During initial landing-site selection, select a tile; ensure **Pin landing site** appears without obscuring vanilla controls.
3. Pin two separated tiles; rotate the globe and zoom in/out. Check alignment, back-face occlusion and overlap with vanilla UI.
4. Remove one pin; confirm the other remains.
5. Save and reload a game; confirm pins persist.
6. Start a different world; confirm the pins from the first world do not appear.
7. Settle on a pinned tile; verify the marker does not affect settlement eligibility.
8. Check Player.log for exceptions and test with common world-map UI mods.

## Known prototype limitations

- Screen-space icons remain a fixed size; they are not perspective-scaled meshes.
- The top-right button may overlap other UI or not appear in every landing workflow.
- Dot-product back-face culling is approximate near the globe limb.
- Each pin is drawn on every repaint; intended for a small shortlist of sites.
