using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;
using Synthesis.Util;

namespace ImmersiveMusicPatcher
{
    /// <summary>
    /// Common interface for records with a MusicType
    /// </summary>
    [CustomAspectInterface(typeof(ICell), typeof(IWorldspace))]
    public interface IHasMusic : ISkyrimMajorRecord, IHasMusicGetter
    {
        new IFormLinkNullable<IMusicTypeGetter> Music { get; }
    }

    /// <summary>
    /// Common interface for records with a MusicType
    /// </summary>
    [CustomAspectInterface(
        typeof(ICell),
        typeof(ICellGetter),
        typeof(IWorldspace),
        typeof(IWorldspaceGetter)
    )]
    public interface IHasMusicGetter : ISkyrimMajorRecordGetter
    {
        IFormLinkNullableGetter<IMusicTypeGetter> Music { get; }
    }

    public sealed record MusicTypes(
        IFormLinkNullableGetter<IMusicTypeGetter> Source,
        IFormLinkNullableGetter<IMusicTypeGetter> Winning
    );

    public class MusicPatcher : IForwardPatcher<IHasMusic, IHasMusicGetter, MusicTypes>
    {
        public static readonly MusicPatcher Instance = new();

        public MusicTypes Analyze(IHasMusicGetter source, IHasMusicGetter target) =>
            new(source.Music, target.Music);

        public void Patch(IHasMusic target, MusicTypes music) => target.Music.SetTo(music.Source);

        public bool ShouldPatch(MusicTypes values) => !values.Winning.Equals(values.Source);
    }
}
