using Stereo.Modules.State;
using Stereo.Utilities.Extensions;
using UnityEngine;

namespace Stereo.Modules;

public class TitleModule(CollapsedState<TitleModule> collapsedState) : BaseModule
{
    public override void OnMusicPlayerGUI(LobbyMusicPlayer lobbyMusicPlayer)
    {
        if (collapsedState.Collapsed)
        {
            OnGUICollapsed(lobbyMusicPlayer);
        }
        else
        {
            OnGUIExpanded(lobbyMusicPlayer);
        }
    }

    private void OnGUIExpanded(LobbyMusicPlayer lobbyMusicPlayer)
    {
        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(
                lobbyMusicPlayer.SongLoadResult is { State: SongLoadState.Loading }
                    ? "Loading" 
                    : lobbyMusicPlayer.SongLoadResult is { State: SongLoadState.FailedToLoad }
                        ? "Failed to load"
                        : lobbyMusicPlayer.CurrentSong == null
                            ? "Waiting for selection"
                            : "Now playing",
                new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperLeft
                });

            GUILayout.Label(
                lobbyMusicPlayer.QueuedSongs.Count != 0
                    ? $"{lobbyMusicPlayer.QueuePosition + 1}/{lobbyMusicPlayer.QueuedSongs.Count}"
                    : "",
                new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperRight
                });
        }
        GUILayout.EndHorizontal();

        var titleText = GetTitleText(lobbyMusicPlayer);

        const int maxFontSize = 20;
        const float titleWidth = 400f;

        var tempTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = maxFontSize,
            padding = new RectOffset(),
            wordWrap = true
        };

        var actualHeight = tempTitleStyle.CalcSize(new GUIContent("M")).y * 2.4f;

        GUILayout.Label(
            titleText,
            new GUIStyle(tempTitleStyle)
            {
                fontSize =
                    GUI.skin.label.CalcFontSize(new GUIContent(titleText), maxFontSize, titleWidth, actualHeight),
                alignment = TextAnchor.MiddleLeft
            },
            GUILayout.MinWidth(titleWidth),
            GUILayout.Height(actualHeight));

        GUILayout.Space(5f);
    }

    private void OnGUICollapsed(LobbyMusicPlayer lobbyMusicPlayer)
    {
        GUILayout.Space(5f);

        var titleText = GetTitleText(lobbyMusicPlayer);

        var queuePositionText = false /*lobbyMusicPlayer.QueuedSongs.Count != 0*/
            ? $" [{lobbyMusicPlayer.QueuePosition + 1}/{lobbyMusicPlayer.QueuedSongs.Count}]"
            : "";

        const int maxFontSize = 18;
        // TODO: `400f` should not be hardcoded.
        const float titleWidth = 400f;

        var tempTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = maxFontSize,
            normal = new GUIStyleState
            {
                textColor = lobbyMusicPlayer.SongLoadResult switch
                {
                    { State: SongLoadState.Loading } => Color.gray,
                    { State: SongLoadState.FailedToLoad } => Color.red,
                    _ => GUI.skin.label.normal.textColor
                }
            },
            wordWrap = true
        };

        var actualHeight = tempTitleStyle.CalcSize(new GUIContent("M")).y * 2f;

        GUILayout.Label(
            $"{titleText}{queuePositionText}",
            new GUIStyle(tempTitleStyle)
            {
                fontSize =
                    GUI.skin.label.CalcFontSize(new GUIContent(titleText), maxFontSize, titleWidth, actualHeight),
                alignment = TextAnchor.MiddleLeft
            },
            GUILayout.Width(400f),
            GUILayout.Height(actualHeight));

        GUILayout.Space(5f);
    }

    private string GetTitleText(LobbyMusicPlayer lobbyMusicPlayer)
    {
        var audioClip = lobbyMusicPlayer.CurrentSong?.AudioClip;

        var titleText = lobbyMusicPlayer.SongLoadResult is { State: SongLoadState.Loading } loadingSongResult
            ? loadingSongResult.Song.Title
            : lobbyMusicPlayer.SongLoadResult is { State: SongLoadState.FailedToLoad } failedToLoadSongResult
                ? failedToLoadSongResult.Song.Title
                : audioClip == null
                    ? "No song selected"
                    : audioClip.name;

        return titleText;
    }
}
