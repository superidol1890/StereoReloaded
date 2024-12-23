using System;
using Mitochondria.Resources.Primitives;
using Mitochondria.Resources.Utilities;
using Reactor.Utilities.Extensions;
using Stereo.Components;
using Stereo.Modules.State;
using Stereo.Utilities.Extensions;
using Stereo.Utilities.Structures;
using UnityEngine;

namespace Stereo.Modules;

public class SettingsModule(SettingsState settingsState) : BaseModule
{
    private bool _visibilityToggleShortcutCaptureKeyboard;
    private bool _previousSongShortcutCaptureKeyboard;
    private bool _nextSongShortcutCaptureKeyboard;
    private bool _toggleSongPlaybackShortcutCaptureKeyboard;
    private KeyboardShortcut? _tempKeyboardShortcut;

    private Vector2 _scrollPosition;

    private bool _discoMode;
    private GameObject? _discoLightGameObject;

    public override void OnMusicPlayerGUI(LobbyMusicPlayer lobbyMusicPlayer)
    {
        if (!settingsState.ShowSettings)
        {
            return;
        }

        var sectionLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        var boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset
            {
                top = 5,
                bottom = 10,
                left = 10,
                right = 10
            }
        };

        GUILayout.Space(5f);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(500f));
        {
            {
                GUILayout.Label(SettingsState.PlaybackSectionTitle, sectionLabelStyle);

                GUILayout.BeginVertical(boxStyle);
                {
                    settingsState.AutoplayOnStart.Value = GUILayout.Toggle(
                        settingsState.AutoplayOnStart.Value,
                        " Autoplay on start");
                }
                GUILayout.EndVertical();

                GUIUtils.KeyboardShortcut(
                    ref _previousSongShortcutCaptureKeyboard,
                    ref _tempKeyboardShortcut,
                    settingsState.PreviousSongShortcut.Value,
                    newShortcut => settingsState.PreviousSongShortcut.Value = newShortcut,
                    Constants.Shortcuts.PreviousSong,
                    "Previous Song Shortcut");

                GUIUtils.KeyboardShortcut(
                    ref _nextSongShortcutCaptureKeyboard,
                    ref _tempKeyboardShortcut,
                    settingsState.NextSongShortcut.Value,
                    newShortcut => settingsState.NextSongShortcut.Value = newShortcut,
                    Constants.Shortcuts.NextSong,
                    "Next Song Shortcut");

                GUIUtils.KeyboardShortcut(
                    ref _toggleSongPlaybackShortcutCaptureKeyboard,
                    ref _tempKeyboardShortcut,
                    settingsState.ToggleSongPlaybackShortcut.Value,
                    newShortcut => settingsState.ToggleSongPlaybackShortcut.Value = newShortcut,
                    Constants.Shortcuts.ToggleSongPlayback,
                    "Toggle Song Playback Shortcut");
            }

            GUILayout.Space(20f);

            {
                GUILayout.Label(SettingsState.VisibilitySectionTitle, sectionLabelStyle);

                GUILayout.BeginVertical(boxStyle);
                {
                    settingsState.ShowMusicPlayerOnStart.Value = GUILayout.Toggle(
                        settingsState.ShowMusicPlayerOnStart.Value,
                        " Show music player on start");
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(boxStyle);
                {
                    settingsState.CollapseMusicPlayerOnStart.Value = GUILayout.Toggle(
                        settingsState.CollapseMusicPlayerOnStart.Value,
                        " Collapse music player on start");
                }
                GUILayout.EndVertical();

                /*{
                    var disabled = settingsState.VisibilityToggleShortcut.Value.KeyCodes.IsEmpty &&
                                   settingsState.ShowHudButton.Value;

                    GUI.enabled = !disabled;

                    var oldShowHudButton = settingsState.ShowHudButton.Value;

                    settingsState.ShowHudButton.Value = GUILayout.Toggle(
                        oldShowHudButton,
                        " Show HUD button");

                    if (settingsState.ShowHudButton.Value != oldShowHudButton &&
                        HudUtils.TryFindHudButton(out var hudButton))
                    {
                        hudButton.SetActive(settingsState.ShowHudButton.Value);
                    }

                    GUI.enabled = true;

                    if (disabled)
                    {
                        GUILayout.Label(
                            "A keyboard shortcut must be set before the HUD button can be hidden.",
                            new GUIStyle(GUI.skin.label)
                            {
                                normal = new GUIStyleState
                                {
                                    textColor = Color.gray
                                },
                                wordWrap = true
                            });

                        GUILayout.Space(10f);
                    }
                }*/

                GUIUtils.KeyboardShortcut(
                    ref _visibilityToggleShortcutCaptureKeyboard,
                    ref _tempKeyboardShortcut,
                    settingsState.VisibilityToggleShortcut.Value,
                    newShortcut => settingsState.VisibilityToggleShortcut.Value = newShortcut,
                    Constants.Shortcuts.ToggleMusicPlayer,
                    "Visibility Toggle Shortcut");
            }

            GUILayout.Space(20f);

            {
                GUILayout.Label(SettingsState.NotificationsSectionTitle, sectionLabelStyle);

                {
                    settingsState.ShowSongNotification.Value = GUIUtils.ToggleGroup(
                        settingsState.ShowSongNotification.Value,
                        @enum =>
                            @enum switch
                            {
                                SettingsState.ShowNotification.No => "No",
                                SettingsState.ShowNotification.Yes => "Yes",
                                SettingsState.ShowNotification.OnlyWhenMusicPlayerHidden =>
                                    "Only When Music Player Hidden",
                                _ => throw new ArgumentOutOfRangeException(nameof(@enum), @enum, null)
                            },
                        "Show Song Notification");
                }
            }
            
            GUILayout.Space(20f);

            {
                var oldDiscoMode = _discoMode;

                _discoMode = GUILayout.Toggle(
                    oldDiscoMode,
                    "Ultra Secret Disco Mode",
                    new GUIStyle(GUI.skin.button)
                    {
                        padding = new RectOffset
                        {
                            top = 15,
                            bottom = 15,
                            left = 15,
                            right = 15
                        }
                    });

                if (_discoMode != oldDiscoMode)
                {
                    if (_discoMode)
                    {
                        if (_discoLightGameObject == null)
                        {
                            const float length = 1200f;
                            const float radius = length / 2f;

                            var texture = TexturePrimitives.GenerateCircle(
                                radius,
                                (x, y) =>
                                {
                                    var opacity = (byte) Math.Min(
                                        255f,
                                        Math.Sqrt(Math.Pow(radius - x, 2) + Math.Pow(radius - y, 2)));

                                    return new Color32(
                                        (byte) (255f * x / length),
                                        (byte) (255f * y / length),
                                        100,
                                        opacity);
                                });

                            var sprite = SpriteUtils.CreateSprite(texture, 100f);

                            _discoLightGameObject = new GameObject("Disco Light");
                            _discoLightGameObject.transform.SetParent(HudManager.Instance.transform);

                            var aspectPosition = _discoLightGameObject.AddComponent<AspectPosition>();
                            aspectPosition.Alignment = AspectPosition.EdgeAlignments.Center;
                            aspectPosition.DistanceFromEdge = Vector3.zero;
                            aspectPosition.updateAlways = true;

                            var spriteRenderer = _discoLightGameObject.AddComponent<SpriteRenderer>();
                            spriteRenderer.sprite = sprite;
                            spriteRenderer.color = new Color32(255, 255, 255, 175);

                            _discoLightGameObject.AddComponent<SpinBehaviour>();
                        }

                        _discoLightGameObject.SetActive(true);
                    }
                    else
                    {
                        if (_discoLightGameObject != null)
                        {
                            _discoLightGameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        GUILayout.EndScrollView();

        GUILayout.Space(5f);
    }

    public override void Dispose()
    {
        if (_discoLightGameObject != null)
        {
            _discoLightGameObject.Destroy();
        }

        GC.SuppressFinalize(this);
    }
}
