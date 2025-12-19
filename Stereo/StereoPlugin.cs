global using static Reactor.Utilities.Logger<Stereo.StereoPlugin>;
using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Mitochondria.Resources;
using Mitochondria.Resources.FFmpeg;
using Reactor.Utilities;
using Stereo.Modules;
using Stereo.Modules.State;

namespace Stereo;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(MitochondriaResourcesPlugin.Id)]
[BepInDependency(MitochondriaResourcesFFmpegPlugin.Id)]
public partial class StereoPlugin : BasePlugin
{
    public static LobbyMusicPlayer? LobbyMusicPlayer { get; internal set; }

    public static bool ShowPlayer { get; set; }

    public static ModuleCollection.ModuleCollectionPointer Modules { get; }

    private static readonly ModuleCollection ModuleCollection = new();

    public Harmony Harmony { get; } = new(Id);

    static StereoPlugin()
    {
        Modules = ModuleCollection.Pointer;
    }

    public override void Load()
    {
        if (!Directory.Exists(Constants.Paths.Songs))
        {
            Directory.CreateDirectory(Constants.Paths.Songs);
        }

        ReactorCredits.Register<StereoPlugin>(ReactorCredits.AlwaysShow);

        Modules
            .Register<CollapsedState<TitleModule>>()
            .Register<CollapsedState<QueueModule>>()
            .Register<SettingsState>()
            .Register<WindowState>()
            .Register<LoadSongsFolderModule>()
            .Register<TitleModule>()
            .Register<NavigationModule>()
            .Register<ProgressBarModule>()
            .Register<VolumeModule>()
            .Register<ToolbarModule>()
            .Register<SettingsModule>()
            .Register<QueueModule>();

        Harmony.PatchAll();
    }

    public override bool Unload()
    {
        Harmony.UnpatchSelf();

        return base.Unload();
    }
}