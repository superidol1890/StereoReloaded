using System;
using System.Diagnostics;
using System.IO;
using Reactor.Utilities;
using Reactor.Utilities.ImGui;
using UnityEngine;

namespace Stereo.Modules;

public class QueueModule : BaseModule
{
    public DragWindow Window { get; }

    private Vector2 _scrollPosition;
    private bool _autoScroll = true;

    public QueueModule()
    {
        Window = new DragWindow(new Rect(470f, 20f, 470f, 0f), "Queue", OnWindow);
    }

    private void OnWindow()
    {
        if (StereoPlugin.LobbyMusicPlayer is not { } lobbyMusicPlayer)
        {
            return;
        }

        const float scrollViewHeight = 250f;

        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset
            {
                top = 10,
                bottom = 10,
                left = 10,
                right = 10
            }
        };

        var numberLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleRight,
            padding = new RectOffset
            {
                top = 15,
                bottom = 15,
                left = 5,
                right = 5
            }
        };

        var selectedNumberLabelStyle = new GUIStyle(numberLabelStyle)
        {
            fontStyle = FontStyle.Bold
        };

        var songLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset
            {
                top = 15,
                bottom = 15,
                left = 10,
                right = 10
            }
        };

        var songButtonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset
            {
                top = 15,
                bottom = 15,
                left = 10,
                right = 10
            }
        };

        // This is all very delicate and prone to breaking.
        var numberLabelWidth = numberLabelStyle.CalcSize(new GUIContent("00")).x;

        var songHeight = songButtonStyle.CalcHeight(new GUIContent("Thick Of It"), 400f) +
                         songButtonStyle.margin.top;

        var selectedSongHeight = songLabelStyle.CalcHeight(new GUIContent("Skibidi Toilet"), 400f)
                                 + songButtonStyle.margin.top
                                 + songButtonStyle.margin.bottom
                                 + GUI.skin.box.margin.top
                                 + GUI.skin.box.margin.bottom;

        var allSongsHeight = songHeight * (lobbyMusicPlayer.QueuedSongs.Count - 1) + selectedSongHeight;
        var maxScrollHeight = Math.Max(0f, allSongsHeight - scrollViewHeight);

        var autoScrollY = Math.Min(maxScrollHeight, songHeight * lobbyMusicPlayer.QueuePosition);

        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        {
            var autoScrollStatusText = _autoScroll ? "ON" : "OFF";
            _autoScroll = GUILayout.Toggle(_autoScroll, $"Auto Scroll: {autoScrollStatusText}", buttonStyle);

            if (GUILayout.Button("Open Songs Folder", buttonStyle))
            {
                Process.Start("explorer.exe", Constants.Paths.Songs + Path.DirectorySeparatorChar);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);

        {
            if (_autoScroll)
            {
                _scrollPosition.y = autoScrollY;
            }

            var oldScrollY = _scrollPosition.y;

            _scrollPosition = GUILayout.BeginScrollView(
                _scrollPosition,
                false,
                true,
                GUILayout.Width(460f),
                GUILayout.Height(scrollViewHeight),
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(false));
            {
                for (var i = 0; i < lobbyMusicPlayer.QueuedSongs.Count; i++)
                {
                    var song = lobbyMusicPlayer.QueuedSongs[i];
                    var isSelected = lobbyMusicPlayer.QueuePosition == i;

                    var songStyle = isSelected ? GUI.skin.box : GUIStyle.none;

                    GUILayout.BeginHorizontal(songStyle);
                    {
                        GUILayout.Label(
                            $"{i + 1}",
                            isSelected ? selectedNumberLabelStyle : numberLabelStyle,
                            GUILayout.Width(numberLabelWidth));

                        GUILayout.FlexibleSpace();

                        if (isSelected)
                        {
                            GUILayout.Label(
                                song.Metadata.Title,
                                songLabelStyle,
                                GUILayout.Width(400f),
                                GUILayout.ExpandWidth(false));
                        }
                        else
                        {
                            if (GUILayout.Button(
                                    song.Metadata.Title,
                                    songButtonStyle,
                                    GUILayout.Width(400f),
                                    GUILayout.ExpandWidth(false)))
                            {
                                Coroutines.Start(lobbyMusicPlayer.PlayInQueue(i));
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();

            if (!Mathf.Approximately(_scrollPosition.y, oldScrollY))
            {
                _autoScroll = false;
            }
        }

        GUILayout.Space(5f);
    }
}
