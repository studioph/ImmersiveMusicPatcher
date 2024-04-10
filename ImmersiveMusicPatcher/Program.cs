using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;

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
            if (immersiveMusicEsp.Mod == null) return;
            var masterCells = immersiveMusicEsp.Mod.Cells;
            var masterWorldspaces = immersiveMusicEsp.Mod.Worldspaces;
            // var ocwPatchEsp = "OCW_MusicPatch_IM.esp";

            // Check if cell music type matches one in immersive music otherwise deep copy and set in patcher
            foreach (var masterCellBlockGetter in masterCells.Records) {
                foreach (var masterCellSubBlock in masterCellBlockGetter.SubBlocks) {
                    foreach (var immersiveMusicCell in masterCellSubBlock.Cells)
                    {
                        patchCell(immersiveMusicCell, state);
                    }
                }
            }
        }

        private static void patchCell(ICellGetter cell, IPatcherState<ISkyrimMod, ISkyrimModGetter> state){
            if (!cell
                .ToLink()
                .TryResolveContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(
                    state.LinkCache,
                    out var winningCellContext)
            ) continue;

            // Don't patch if music type matches existing
            if (cell.Music.Equals(winningCellContext.Record.Music))
            {
                continue;
            }

            var patchCell = winningCellContext.GetOrAddAsOverride(state.PatchMod);
            if (!cell.Music.IsNull) {
                patchCell.Music.FormKey = cell.Music.FormKey;
            }
        }
    }
}
