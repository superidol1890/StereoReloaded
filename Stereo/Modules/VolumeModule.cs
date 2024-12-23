using Stereo.Modules.State;
using UnityEngine;

namespace Stereo.Modules;

public class VolumeModule(CollapsedState<TitleModule> collapsedState) : BaseModule
{
    public override void OnMusicPlayerGUI(LobbyMusicPlayer lobbyMusicPlayer)
    {
        var padding = collapsedState.Collapsed ? 2f : 5f;

        GUILayout.Space(padding);

        GUILayout.BeginHorizontal();
        {
            var volumeLabelSize = GUI.skin.label.CalcSize(new GUIContent("Volume: 100%"));

            GUILayout.Label($"Volume: {lobbyMusicPlayer.Volume}%", GUILayout.Width(volumeLabelSize.x));

            GUILayout.Space(20f);

            GUILayout.BeginVertical();
            {
                GUILayout.Space(volumeLabelSize.y / 2);
                lobbyMusicPlayer.Volume = (int) GUILayout.HorizontalSlider(lobbyMusicPlayer.Volume, 0, 100);
            }
            GUILayout.EndVertical();

            GUILayout.Space(20f);

            if (GUILayout.Button("-", GUILayout.Width(50f)))
            {
                lobbyMusicPlayer.Volume--;
            }

            if (GUILayout.Button("+", GUILayout.Width(50f)))
            {
                lobbyMusicPlayer.Volume++;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(padding);
    }
}
