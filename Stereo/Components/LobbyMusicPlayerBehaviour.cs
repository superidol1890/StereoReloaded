using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using Reactor.Utilities.ImGui;
using Stereo.Modules;
using UnityEngine;

namespace Stereo.Components;

[RegisterInIl2Cpp]
public class LobbyMusicPlayerBehaviour : MonoBehaviour
{
    private readonly DragWindow _mainWindow;

    private ServiceProvider? _serviceProvider;
    private BaseModule[] _modules = [];
    private CancellationTokenSource? _initializationCancellationTokenSource;

    public LobbyMusicPlayerBehaviour(IntPtr ptr) : base(ptr)
    {
        _mainWindow = new DragWindow(
            new Rect(20f, 20f, 400f, 0f),
            "Stereo",
            () =>
            {
                if (StereoPlugin.LobbyMusicPlayer is not { } lobbyMusicPlayer)
                {
                    return;
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5f);

                    GUILayout.BeginVertical();
                    {
                        foreach (var module in _modules)
                        {
                            module.OnMusicPlayerGUI(lobbyMusicPlayer);
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.Space(5f);
                }
                GUILayout.EndHorizontal();
            });
    }

    private void Start()
    {
        if (StereoPlugin.LobbyMusicPlayer is not { } lobbyMusicPlayer)
        {
            this.Destroy();
            return;
        }

        var baseModuleType = typeof(BaseModule);
        var serviceCollection = new ServiceCollection();

        foreach (var moduleType in StereoPlugin.Modules.ModuleTypes)
        {
            if (!baseModuleType.IsAssignableFrom(moduleType))
            {
                Error($"Module collection contains non-module type: {moduleType}");
            }

            serviceCollection.AddSingleton(moduleType);

            serviceCollection.AddSingleton<BaseModule>(
                provider => (BaseModule) provider.GetRequiredService(moduleType));
        }

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _modules = _serviceProvider.GetServices<BaseModule>().ToArray();

        _initializationCancellationTokenSource = new CancellationTokenSource();
        var initializationCancellationToken = _initializationCancellationTokenSource.Token;

        foreach (var module in _modules)
        {
            module.Start(lobbyMusicPlayer, initializationCancellationToken);
        }

        Coroutines.Start(lobbyMusicPlayer.Play());
    }

    private void OnGUI()
    {
        if (!StereoPlugin.ShowPlayer || StereoPlugin.LobbyMusicPlayer is not { } lobbyMusicPlayer)
        {
            return;
        }

        foreach (var module in _modules)
        {
            module.OnGUI(lobbyMusicPlayer);
        }

        _mainWindow.OnGUI();
    }

    private void OnDestroy()
    {
        _initializationCancellationTokenSource?.Token.Register(() => _initializationCancellationTokenSource.Dispose());
        _initializationCancellationTokenSource?.Cancel();

        _serviceProvider?.Dispose();
        Array.Clear(_modules);

        Coroutines.Start(StereoPlugin.LobbyMusicPlayer?.Stop());
        StereoPlugin.LobbyMusicPlayer = null;
        StereoPlugin.ShowPlayer = false;
    }
}
