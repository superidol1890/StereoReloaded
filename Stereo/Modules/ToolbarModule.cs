using System;
using Mitochondria.Resources;
using Stereo.Modules.State;
using UnityEngine;

namespace Stereo.Modules;

public class ToolbarModule : BaseModule
{
    private readonly ResourceHandle<Texture2D> _upArrowTextureHandle;
    private readonly ResourceHandle<Texture2D> _downArrowTextureHandle;
    private readonly ResourceHandle<Texture2D> _queueTextureHandle;
    private readonly ResourceHandle<Texture2D> _wrenchTextureHandle;

    private readonly CollapsedState<TitleModule> _collapsedState;
    private readonly SettingsState _settingsState;
    private readonly WindowState _windowState;
    private readonly QueueModule _queueModule;

    public ToolbarModule(
        CollapsedState<TitleModule> collapsedState,
        SettingsState settingsState,
        WindowState windowState,
        QueueModule queueModule)
    {
        _collapsedState = collapsedState;
        _settingsState = settingsState;
        _windowState = windowState;
        _queueModule = queueModule;

        _upArrowTextureHandle = Resources.LobbyMusicPlayer.Buttons.Arrow.UpTextureProvider.AcquireHandle();
        _downArrowTextureHandle = Resources.LobbyMusicPlayer.Buttons.Arrow.DownTextureProvider.AcquireHandle();
        _queueTextureHandle = Resources.LobbyMusicPlayer.Buttons.QueueTextureProvider.AcquireHandle();
        _wrenchTextureHandle = Resources.LobbyMusicPlayer.Buttons.WrenchTextureProvider.AcquireHandle();
    }

    public override void OnMusicPlayerGUI(LobbyMusicPlayer lobbyMusicPlayer)
    {
        GUILayout.Space(20f);

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button(
                    _collapsedState.Collapsed ? _downArrowTextureHandle.Resource : _upArrowTextureHandle.Resource,
                    new GUIStyle(GUI.skin.button)
                    {
                        padding = new RectOffset
                        {
                            top = 10,
                            bottom = 10,
                            left = 10,
                            right = 10
                        }
                    },
                    GUILayout.Width(30f),
                    GUILayout.Height(30f)))
            {
                _collapsedState.Collapsed = !_collapsedState.Collapsed;
            }

            GUILayout.Space(5f);

            {
                var oldVisible = _windowState.SubWindow == _queueModule.Window;

                var visible = GUILayout.Toggle(
                    oldVisible,
                    _queueTextureHandle.Resource,
                    new GUIStyle(GUI.skin.button)
                    {
                        padding = new RectOffset
                        {
                            top = 8,
                            bottom = 8,
                            left = 8,
                            right = 8
                        }
                    },
                    GUILayout.Width(30f),
                    GUILayout.Height(30f));

                if (oldVisible != visible)
                {
                    _windowState.ToggleWindow(_queueModule.Window);
                }
            }

            GUILayout.FlexibleSpace();

            _settingsState.ShowSettings = GUILayout.Toggle(
                _settingsState.ShowSettings,
                _wrenchTextureHandle.Resource,
                new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset
                    {
                        top = 8,
                        bottom = 8,
                        left = 8,
                        right = 8
                    }
                },
                GUILayout.Width(30f),
                GUILayout.Height(30f));
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(_collapsedState.Collapsed ? 2f : 5f);
    }

    public override void Dispose()
    {
        _upArrowTextureHandle.Dispose();
        _downArrowTextureHandle.Dispose();
        _queueTextureHandle.Dispose();
        _wrenchTextureHandle.Dispose();

        GC.SuppressFinalize(this);
    }
}
