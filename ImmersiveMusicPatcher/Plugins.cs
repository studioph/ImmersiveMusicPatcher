using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Synthesis.Util;
using Synthesis.Util.Cell;

namespace ImmersiveMusicPatcher;

/// <summary>
/// Generic Plugin for patching music types from a specific mod
/// </summary>
/// <param name="mod">The mod to use as source</param>
/// <param name="pipeline">The pipeline to patch records through</param>
public class MusicPatcherPlugin(ISkyrimModGetter mod, SkyrimForwardPipeline pipeline)
    : IPatcherPlugin<ISkyrimMod, ISkyrimModGetter>
{
    private readonly ISkyrimModGetter _mod = mod;

    private readonly SkyrimForwardPipeline _pipeline = pipeline;

    public void Run(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        var cellsToPatch = _mod.GetAllCells()
            .Where(cell => cell.Music is not null)
            .Select(cell =>
                cell.WithContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache)
            );

        _pipeline.Run(CellMusicPatcher.Instance, cellsToPatch);

        var worldspacesToPatch = _mod
            .Worldspaces.Where(worldspace => worldspace.Music is not null)
            .Select(worldspace =>
                worldspace.WithContext<
                    ISkyrimMod,
                    ISkyrimModGetter,
                    IWorldspace,
                    IWorldspaceGetter
                >(state.LinkCache)
            );

        _pipeline.Run(WorldspaceMusicPatcher.Instance, worldspacesToPatch);
    }
}

/// <summary>
/// Plugin for the OCW Immersive Music Patch
/// </summary>
/// <param name="mod"></param>
/// <param name="pipeline"></param>
public class OCWPlugin(ISkyrimModGetter mod, SkyrimForwardPipeline pipeline)
    : MusicPatcherPlugin(mod, pipeline),
        IPluginData
{
    private readonly ISkyrimModGetter _mod = mod;
    private readonly SkyrimForwardPipeline _pipeline = pipeline;

    private static readonly ModKey OCW_MusicPatch_IM = ModKey.FromNameAndExtension(
        "OCW_MusicPatch_IM.esp"
    );

    public static PluginData Data => new(OCW_MusicPatch_IM);
}
