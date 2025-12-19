using System;
using Mitochondria.Resources;
using Reactor.Utilities;
using Stereo.Modules.State;
using UnityEngine;

namespace Stereo.Modules;

public class NavigationModule : BaseModule
{
    private readonly ResourceHandle<Texture2D> _playTextureHandle;
    private readonly ResourceHandle<Texture2D> _pauseTextureHandle;
    private readonly ResourceHandle<Texture2D> _reloadTextureHandle;
    private readonly ResourceHandle<Texture2D> _previousTextureHandle;
    private readonly ResourceHandle<Texture2D> _nextTextureHandle;

    private readonly CollapsedState<TitleModule> _collapsedState;

    public NavigationModule(CollapsedState<TitleModule> collapsedState)
    {
        _collapsedState = collapsedState;

        _playTextureHandle = Resources.LobbyMusicPlayer.Buttons.PlayTextureProvider.AcquireHandle();
        _pauseTextureHandle = Resources.LobbyMusicPlayer.Buttons.PauseTextureProvider.AcquireHandle();
        _reloadTextureHandle = Resources.LobbyMusicPlayer.Buttons.ReloadTextureProvider.AcquireHandle();
        _previousTextureHandle = Resources.LobbyMusicPlayer.Buttons.PreviousTextureProvider.AcquireHandle();
        _nextTextureHandle = Resources.LobbyMusicPlayer.Buttons.NextTextureProvider.AcquireHandle();
    }

    public override void OnMusicPlayerGUI(LobbyMusicPlayer lobbyMusicPlayer)
    {
        const float collapsedButtonHeight = 30f;
        var padding = _collapsedState.Collapsed ? 2f : 5f;

        GUILayout.Space(padding);

        GUILayout.BeginHorizontal();
        {
            if (_collapsedState.Collapsed
                    ? GUILayout.Button(_previousTextureHandle.Resource, GUILayout.Height(collapsedButtonHeight))
                    : GUILayout.Button(_previousTextureHandle.Resource))
            {
                Coroutines.Start(lobbyMusicPlayer.PlayNextInQueue(-1, 1f));
            }

            var playTexture = lobbyMusicPlayer.Paused ? _playTextureHandle : _pauseTextureHandle;

            if (_collapsedState.Collapsed
                    ? GUILayout.Button(playTexture.Resource, GUILayout.Height(collapsedButtonHeight))
                    : GUILayout.Button(playTexture.Resource))
            {
                if (lobbyMusicPlayer.Paused)
                {
                    Coroutines.Start(lobbyMusicPlayer.Play());
                }
                else
                {
                    lobbyMusicPlayer.Pause();
                }
            }

            if (_collapsedState.Collapsed
                    ? GUILayout.Button(_nextTextureHandle.Resource, GUILayout.Height(collapsedButtonHeight))
                    : GUILayout.Button(_nextTextureHandle.Resource))
            {
                Coroutines.Start(lobbyMusicPlayer.PlayNextInQueue(1, 1f));
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(padding);
    }

    public override void Dispose()
    {
        _playTextureHandle.Dispose();
        _pauseTextureHandle.Dispose();
        _reloadTextureHandle.Dispose();
        _previousTextureHandle.Dispose();
        _nextTextureHandle.Dispose();

        GC.SuppressFinalize(this);
    }
}
