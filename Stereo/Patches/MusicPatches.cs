using HarmonyLib;
using Stereo.Components;

namespace Stereo.Patches;

internal static class MusicPatches
{
    private static bool _shouldStopMusic;

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.CrossFadeSound))]
    public static class StopDefaultMusicPatch
    {
        public static bool Prefix(string name)
        {
            var shouldPlayMusic = !_shouldStopMusic && name == LobbyBehaviour.MAP_THEME_NAME;
            _shouldStopMusic = false;
            return shouldPlayMusic;
        }
    }

    [HarmonyPatch(typeof(LobbyBehaviour._DelayPlayDropshipAmbience_d__15), nameof(LobbyBehaviour._DelayPlayDropshipAmbience_d__15.MoveNext))]
    public static class SetUpLobbyMusicPlayerPatch
    {
        public static void Postfix(LobbyBehaviour._DelayPlayDropshipAmbience_d__15 __instance)
        {
            // 1 and -1 consistently represent the first and last states, thus mimicking prefixes and postfixes
            switch (__instance.__1__state)
            {
                case 1:
                {
                    _shouldStopMusic = true;
                    break;
                }
                case -1:
                {
                    var lobbyMusicPlayer = new LobbyMusicPlayer();
                    StereoPlugin.LobbyMusicPlayer = lobbyMusicPlayer;

                    __instance.__4__this.gameObject.AddComponent<LobbyMusicPlayerBehaviour>();

                    break;
                }
            }
        }
    }
}
