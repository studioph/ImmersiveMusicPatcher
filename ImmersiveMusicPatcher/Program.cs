using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Synthesis.Util;

namespace ImmersiveMusicPatcher
{
    public class Program
    {
        private static readonly ModKey ImmersiveMusic = ModKey.FromNameAndExtension(
            "Immersive Music.esp"
        );

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline
                .Instance.AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "ImmersiveMusicPatcher.esp")
                .AddRunnabilityCheck(state =>
                {
                    state.LoadOrder.AssertListsMod(
                        ImmersiveMusic,
                        $"\n\nMissing {ImmersiveMusic}\n\n"
                    );
                })
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var immersiveMusic = state.LoadOrder.GetIfEnabledAndExists(ImmersiveMusic);
            var affectedCells = immersiveMusic.Cells;
            var affectedWorldspaces = immersiveMusic.Worldspaces;

            var pipeline = new SkyrimForwardPipeline(state.PatchMod);

            var interiorCellsToPatch = affectedCells
                .Records.SelectMany(cellBlock => cellBlock.SubBlocks)
                .SelectMany(subBlock => subBlock.Cells)
                .Where(cell => cell.Music is not null);

            var worldspaceCellsToPatch = affectedWorldspaces
                .Records.SelectMany(worldspace => worldspace.SubCells)
                .SelectMany(worldspaceBlock => worldspaceBlock.Items)
                .SelectMany(worldspaceSubBlock => worldspaceSubBlock.Items)
                .Where(cell => cell.Music is not null);

            var cellsContextsToPatch = interiorCellsToPatch
                .Concat(worldspaceCellsToPatch)
                .Select(cell =>
                    cell.WithContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(
                        state.LinkCache
                    )
                );

            pipeline.Run(CellMusicPatcher.Instance, cellsContextsToPatch);

            var worldspacesToPatch = affectedWorldspaces
                .Records.Where(worldspace => worldspace.Music is not null)
                .Select(worldspace =>
                    worldspace.WithContext<
                        ISkyrimMod,
                        ISkyrimModGetter,
                        IWorldspace,
                        IWorldspaceGetter
                    >(state.LinkCache)
                );

            pipeline.Run(WorldspaceMusicPatcher.Instance, worldspacesToPatch);
        }
    }
}
