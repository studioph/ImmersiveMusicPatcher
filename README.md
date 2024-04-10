# Immersive Music Patcher

Forwards MusicType records from [Immersive Music](https://www.nexusmods.com/skyrimspecialedition/mods/16402)
to winning cell and worldspace records. This patcher is effectively a clone of [Acoustic Space Improvements Patcher](https://github.com/aglowinthefield/AcousticSpaceImprovementsPatcher), but for Immersive Music.

## Special cases

### Obscure's College of Winterhold
OCW already contains a patch for IM that merges the IM tracks with the OCW tracks. If this patch is detected in the load order, the patcher will use `OCW_MusType` as the MusicType for winning records instead of `MusCollege`.
