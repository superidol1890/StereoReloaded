using System;
using Stereo.Modules.State;
using UnityEngine;

namespace Stereo.Modules;

public class ProgressBarModule(CollapsedState<TitleModule> collapsedState) : BaseModule
{
    public override void OnMusicPlayerGUI(LobbyMusicPlayer lobbyMusicPlayer)
    {
        var padding = collapsedState.Collapsed ? 2f : 5f;

        GUILayout.Space(padding);

        GUILayout.BeginHorizontal();
        {
            var format = lobbyMusicPlayer.Duration >= 3600 ? @"hh\:mm\:ss" : @"mm\:ss";
            var duration = TimeSpan.FromSeconds(lobbyMusicPlayer.Duration).ToString(format);
            var playbackTime = TimeSpan.FromSeconds(lobbyMusicPlayer.PlaybackTime).ToString(format);

            GUILayout.Label(
                $"{playbackTime} / {duration}",
                GUILayout.ExpandWidth(false));

            GUILayout.Space(20f);

            GUILayout.BeginVertical();
            {
                GUILayout.Space(GUI.skin.label.CalcSize(new GUIContent("00:00 / 00:00")).y / 2f);

                var newPlaybackTime = GUILayout.HorizontalSlider(
                    lobbyMusicPlayer.PlaybackTime,
                    0,
                    lobbyMusicPlayer.Duration,
                    GUILayout.ExpandWidth(true));

                if (Math.Abs(newPlaybackTime - lobbyMusicPlayer.PlaybackTime) > 0.1f)
                {
                    lobbyMusicPlayer.PlaybackTime = Math.Clamp(newPlaybackTime, 0, lobbyMusicPlayer.Duration);
                }
            }
            GUILayout.EndVertical();

            if (collapsedState.Collapsed)
            {
                GUILayout.Space(20f);

                var loopStatusText = lobbyMusicPlayer.Loop ? "ON" : "OFF";

                lobbyMusicPlayer.Loop = GUILayout.Toggle(
                    lobbyMusicPlayer.Loop,
                    $"Loop: {loopStatusText}",
                    GUI.skin.button,
                    GUILayout.Width(90f));
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);

        if (!collapsedState.Collapsed)
        {
            lobbyMusicPlayer.Loop = GUILayout.Toggle(lobbyMusicPlayer.Loop, " Loop");
        }

        GUILayout.Space(padding);
    }
}
