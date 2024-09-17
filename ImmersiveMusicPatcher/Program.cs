using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace ImmersiveMusicPatcher
{
    public class Program
    {
        private static readonly ModKey ImmersiveMusic = ModKey.FromNameAndExtension("Immersive Music.esp");

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "ImmersiveMusicPatcher.esp")
                .AddRunnabilityCheck(state =>
                {
                    state.LoadOrder.AssertListsMod(ImmersiveMusic, "\n\nMissing Immersive Music.esp\n\n");
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
            var affectedCells = immersiveMusicEsp.Mod.Cells;
            var affectedWorldspaces = immersiveMusicEsp.Mod.Worldspaces;

            var interiorCellsToPatch = affectedCells.Records
                .SelectMany(cellBlock => cellBlock.SubBlocks)
                .SelectMany(subBlock => subBlock.Cells)
                .Where(cell => cell.Music is not null);
            interiorCellsToPatch.ForEach(cell => PatchCell(cell, state));

            var worldspaceCellsToPatch = affectedWorldspaces.Records
                .SelectMany(worldspace => worldspace.SubCells)
                .SelectMany(worldspaceBlock => worldspaceBlock.Items)
                .SelectMany(worldspaceSubBlock => worldspaceSubBlock.Items)
                .Where(cell => cell.Music is not null);
            worldspaceCellsToPatch.ForEach(cell => PatchCell(cell, state));

            affectedWorldspaces.Records
            .Where(worldspace => worldspace.Music is not null)
            .ForEach(worldspace => PatchWorldspace(worldspace, state));
        }

        private static void PatchWorldspace(IWorldspaceGetter worldspace, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Check if worldspace music type matches one in immersive music otherwise deep copy and set in patcher
            if (!worldspace
                .ToLink()
                .TryResolveContext<ISkyrimMod, ISkyrimModGetter, IWorldspace, IWorldspaceGetter>(
                    state.LinkCache,
                    out var winningWorldspace)
            )
            {
                Console.WriteLine($"WARNING: Unable to resolve FormKey: {worldspace.FormKey}, skipping worldspace");
                return;
            }

            // Don't patch if music type already matches
            if (worldspace.Music.Equals(winningWorldspace.Record.Music))
            {
                return;
            }

            var patchWorldspace = winningWorldspace.GetOrAddAsOverride(state.PatchMod);
            patchWorldspace.Music.FormKey = worldspace.Music.FormKey;
            Console.WriteLine($"Patched MusicType for worldspace {worldspace}");
        }

        private static void PatchCell(ICellGetter cell, IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Check if cell music type matches one in immersive music otherwise deep copy and set in patcher
            if (!cell
                .ToLink()
                .TryResolveContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(
                    state.LinkCache,
                    out var winningCellContext)
            )
            {
                Console.WriteLine($"WARNING: Unable to resolve FormKey: {cell.FormKey}, skipping cell");
                return;
            }

            // Don't patch if music type already matches
            if (cell.Music.Equals(winningCellContext.Record.Music))
            {
                return;
            }

            var patchCell = winningCellContext.GetOrAddAsOverride(state.PatchMod);
            patchCell.Music.FormKey = cell.Music.FormKey;
            Console.WriteLine($"Patched MusicType for cell {cell}");
        }
    }
}
