using System;
using HarmonyLib;
using Mitochondria.Resources;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Stereo.Patches;

internal static class HudPatches
{
    private static Transform? _musicButtonTransform;

    private static ResourceHandle<Sprite>? _inactiveSpriteHandle;
    private static ResourceHandle<Sprite>? _activeSpriteHandle;
    private static ResourceHandle<Sprite>? _selectedSpriteHandle;

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    public static class CreateMusicButtonPatch
    {
        public static void Postfix()
        {
            var settingsButton = GameObject.Find("Hud/Buttons/TopRight/MenuButton");
            if (settingsButton == null)
            {
                return;
            }

            _inactiveSpriteHandle = Resources.LobbyMusicPlayer.Buttons.Note.InactiveSpriteProvider.AcquireHandle();
            _activeSpriteHandle = Resources.LobbyMusicPlayer.Buttons.Note.ActiveSpriteProvider.AcquireHandle();
            _selectedSpriteHandle = Resources.LobbyMusicPlayer.Buttons.Note.SelectedSpriteProvider.AcquireHandle();

            var settingsButtonTransform = settingsButton.transform;

            _musicButtonTransform = Object.Instantiate(settingsButtonTransform, settingsButtonTransform.parent);
            _musicButtonTransform.name = "MusicButton";

            _musicButtonTransform.GetComponent<ControllerButtonBehavior>().Destroy();

            SetMusicButtonSprite("Inactive", _inactiveSpriteHandle.Resource);
            SetMusicButtonSprite("Active", _activeSpriteHandle.Resource);
            SetMusicButtonSprite("Selected", _selectedSpriteHandle.Resource);

            var aspectPosition = _musicButtonTransform.GetComponent<AspectPosition>();
            var distanceFromEdge = aspectPosition.DistanceFromEdge;
            distanceFromEdge.x = 2.74f;
            aspectPosition.DistanceFromEdge = distanceFromEdge;

            var passiveButton = _musicButtonTransform.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((Action) (() =>
            {
                StereoPlugin.ShowPlayer = !StereoPlugin.ShowPlayer;
                passiveButton.SelectButton(StereoPlugin.ShowPlayer);
            }));

            return;

            void SetMusicButtonSprite(string state, Sprite sprite)
            {
                _musicButtonTransform!.Find(state).GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Start))]
    public static class HideMusicPlayerPatch
    {
        public static void Postfix(PlayerCustomizationMenu __instance)
        {
            var originalShowPlayer = StereoPlugin.ShowPlayer;
            StereoPlugin.ShowPlayer = false;

            if (_musicButtonTransform != null)
            {
                _musicButtonTransform.gameObject.SetActive(false);
            }

            __instance.OnClose += (Action) (() =>
            {
                StereoPlugin.ShowPlayer = originalShowPlayer;

                if (_musicButtonTransform != null)
                {
                    _musicButtonTransform.gameObject.SetActive(true);
                }
            });
        }
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.OnDestroy))]
    public static class DestroyMusicButtonPatch
    {
        public static void Postfix()
        {
            _inactiveSpriteHandle?.Dispose();
            _activeSpriteHandle?.Dispose();
            _selectedSpriteHandle?.Dispose();

            if (_musicButtonTransform != null)
            {
                _musicButtonTransform.gameObject.Destroy();
            }
        }
    }
}
