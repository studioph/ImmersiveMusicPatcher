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
        var cellsToPatch = _mod.GetAllCells().Cast<IHasMusicGetter>();
        var worldspacesToPatch = _mod.Worldspaces.Cast<IHasMusicGetter>();

        var recordsToPatch = cellsToPatch
            .Concat(worldspacesToPatch)
            .Where(record => record.Music is not null)
            .Select(record =>
                record.WithContext<ISkyrimMod, ISkyrimModGetter, IHasMusic, IHasMusicGetter>(
                    state.LinkCache
                )
            );

        _pipeline.Run(MusicPatcher.Instance, recordsToPatch);
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
    private static readonly ModKey OCW_CellSettings = ModKey.FromNameAndExtension(
        "OCW_CellSettings.esp"
    );

    public static PluginData Data => new(nameof(OCWPlugin), OCW_MusicPatch_IM, OCW_CellSettings);
}
