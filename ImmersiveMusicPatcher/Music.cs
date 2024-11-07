using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Synthesis.Util;

namespace ImmersiveMusicPatcher
{
    /// <summary>
    /// DTO Containing a reference (source) and winning MusicType values for a record
    /// </summary>
    /// <param name="Source">A reference value from a particular mod</param>
    /// <param name="Winning">The value from the winning record</param>
    public sealed record MusicTypes(
        IFormLinkNullableGetter<IMusicTypeGetter> Source,
        IFormLinkNullableGetter<IMusicTypeGetter> Winning
    );

    // Need 2 almost identical implementations due to lack of aspect interface
    public class CellMusicPatcher : IForwardPatcher<ICell, ICellGetter, MusicTypes>
    {
        public static readonly CellMusicPatcher Instance = new();

        public MusicTypes Analyze(ICellGetter source, ICellGetter target) =>
            new(source.Music, target.Music);

        public void Patch(ICell target, MusicTypes music) =>
            target.Music.FormKey = music.Source.FormKey;

        public bool ShouldPatch(MusicTypes values) => !values.Winning.Equals(values.Source);
    }

    public class WorldspaceMusicPatcher
        : IForwardPatcher<IWorldspace, IWorldspaceGetter, MusicTypes>
    {
        public static readonly WorldspaceMusicPatcher Instance = new();

        public MusicTypes Analyze(IWorldspaceGetter source, IWorldspaceGetter target) =>
            new(source.Music, target.Music);

        public void Patch(IWorldspace target, MusicTypes music) =>
            target.Music.FormKey = music.Source.FormKey;

        public bool ShouldPatch(MusicTypes values) => !values.Winning.Equals(values.Source);
    }
}
