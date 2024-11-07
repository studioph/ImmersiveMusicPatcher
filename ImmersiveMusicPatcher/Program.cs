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

        private static SkyrimForwardPipeline _pipeline = null!;

        private static readonly PluginLoader<ISkyrimMod, ISkyrimModGetter> _loader = new();

        // Register optional plugins with loader
        static Program()
        {
            _loader.Register<OCWPlugin>((mod) => new OCWPlugin(mod, _pipeline));
        }

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
            _pipeline = new SkyrimForwardPipeline(state.PatchMod);
            var immersiveMusic = state.LoadOrder.GetIfEnabledAndExists(ImmersiveMusic);
            var mainPlugin = new MusicPatcherPlugin(immersiveMusic, _pipeline);
            var loadedPlugins = _loader.Scan(state.LoadOrder);

            // Patch main IM mod
            mainPlugin.Run(state);

            // Run any optional plugins based on load order
            foreach (var plugin in loadedPlugins)
            {
                plugin.Run(state);
            }

            Console.WriteLine($"Patched {_pipeline.PatchedCount} records");
        }
    }
}
