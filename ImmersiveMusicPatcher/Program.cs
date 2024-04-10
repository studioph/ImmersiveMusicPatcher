using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace ImmersiveMusicPatcher
{
    public class Program
    {
        private static readonly ModKey ImmersiveMusic = ModKey.FromNameAndExtension("ImmersiveMusic.esp");

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "ImmersiveMusicPatcher.esp")
                .AddRunnabilityCheck(state =>
                {
                    state.LoadOrder.AssertListsMod(ImmersiveMusic, "\n\nMissing ImmersiveMusic.esp\n\n");
                })
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var immersiveMusicEsp = state.LoadOrder.GetIfEnabled(ImmersiveMusic);
            if (immersiveMusicEsp.Mod == null)
            {
                return;
            }
            var masterCells = immersiveMusicEsp.Mod.Cells;
            var masterWorldspaces = immersiveMusicEsp.Mod.Worldspaces;
            // var ocwPatchEsp = "OCW_MusicPatch_IM.esp";

            var interiorCellsToPatch = masterCells.Records
                .SelectMany(cellBlock => cellBlock.SubBlocks)
                .SelectMany(subBlock => subBlock.Cells);
            interiorCellsToPatch.ForEach(cell => PatchCell(cell, state));

            var worldspaceCellsToPatch = masterWorldspaces.Records
                .SelectMany(worldspace => worldspace.SubCells)
                .SelectMany(worldspaceBlock => worldspaceBlock.Items)
                .SelectMany(worldspaceSubBlock => worldspaceSubBlock.Items);
            worldspaceCellsToPatch.ForEach(cell => PatchCell(cell, state));
        }

        private static void PatchCell(ICellGetter cell, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Check if cell music type matches one in immersive music otherwise deep copy and set in patcher
            if (!cell
                .ToLink()
                .TryResolveContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(
                    state.LinkCache,
                    out var winningCellContext)
            ) return;

            // Don't patch if music type matches existing
            if (cell.Music.Equals(winningCellContext.Record.Music))
            {
                return;
            }

            var patchCell = winningCellContext.GetOrAddAsOverride(state.PatchMod);
            if (!cell.Music.IsNull)
            {
                patchCell.Music.FormKey = cell.Music.FormKey;
            }
        }
    }
}
