using System;
using System.Collections.Generic;
using System.Threading;
using BepInEx.Configuration;
using Mitochondria.Input;
using Mitochondria.Resources;
using Reactor.Utilities;
using Stereo.Utilities;
using Stereo.Utilities.Structures;
using UnityEngine;

namespace Stereo.Modules.State;

public class SettingsState : BaseModule
{
    public bool ShowSettings { get; set; }

    public ConfigEntry<ShowNotification> ShowSongNotification { get; }

    public ConfigEntry<bool> AutoplayOnStart { get; }

    public ConfigEntry<KeyboardShortcut> PreviousSongShortcut { get; }

    public ConfigEntry<KeyboardShortcut> NextSongShortcut { get; }

    public ConfigEntry<KeyboardShortcut> ToggleSongPlaybackShortcut { get; }

    public ConfigEntry<bool> ShowMusicPlayerOnStart { get; }

    public ConfigEntry<bool> CollapseMusicPlayerOnStart { get; }

    public ConfigEntry<bool> ShowHudButton { get; }

    public ConfigEntry<KeyboardShortcut> VisibilityToggleShortcut { get; }

    public ConfigFile Config => _config ??= PluginSingleton<StereoPlugin>.Instance.Config;

    public const string NotificationsSectionTitle = "Notifications";
    public const string PlaybackSectionTitle = "Playback";
    public const string VisibilitySectionTitle = "Visibility";

    private ConfigFile? _config;

    private readonly ResourceHandle<Sprite> _noteSilhouetteSpriteHandle;

    private readonly CollapsedState<TitleModule> _collapsedState;

    public SettingsState(CollapsedState<TitleModule> collapsedState)
    {
        _collapsedState = collapsedState;

        _noteSilhouetteSpriteHandle = Resources.LobbyMusicPlayer.NoteSilhouetteSpriteProvider.AcquireHandle();

        if (TomlTypeConverter.GetConverter(typeof(KeyboardShortcut)) == null)
        {
            TomlTypeConverter.AddConverter(typeof(KeyboardShortcut), KeyboardShortcutConverterFactory.Create());
        }

        ShowSongNotification = Config.Bind(
            NotificationsSectionTitle,
            "ShowSongNotification",
            ShowNotification.No,
            "Whether or not a notification should be displayed when advancing to the next song.");

        AutoplayOnStart = Config.Bind(
            PlaybackSectionTitle,
            "AutoplayOnStart",
            true,
            "Whether or not the music player should start playing automatically when entering the lobby area.");

        PreviousSongShortcut = Config.Bind(
            PlaybackSectionTitle,
            "PreviousSongShortcut",
            Constants.Shortcuts.PreviousSong,
            "The shortcut for playing the previous song in the music player queue.");

        NextSongShortcut = Config.Bind(
            PlaybackSectionTitle,
            "NextSongShortcut",
            Constants.Shortcuts.NextSong,
            "The shortcut for playing the next song in the music player queue.");

        ToggleSongPlaybackShortcut = Config.Bind(
            PlaybackSectionTitle,
            "PlayPauseSongShortcut",
            Constants.Shortcuts.ToggleSongPlayback,
            "The shortcut for playing/pausing the current song.");

        ShowMusicPlayerOnStart = Config.Bind(
            VisibilitySectionTitle,
            "ShowMusicPlayerOnStart",
            true,
            "Whether or not the music player should be visible when entering the lobby area.");

        CollapseMusicPlayerOnStart = Config.Bind(
            VisibilitySectionTitle,
            "CollapseMusicPlayerOnStart",
            false,
            "Whether or not the music player should be collapsed when entering the lobby area.");

        ShowHudButton = Config.Bind(
            VisibilitySectionTitle,
            "ShowHudButton",
            true,
            "Whether or not the HUD music button should be visible.");

        VisibilityToggleShortcut = Config.Bind(
            VisibilitySectionTitle,
            "VisibilityToggleShortcut",
            Constants.Shortcuts.ToggleMusicPlayer,
            "The shortcut for showing/hiding the music player.");
    }

    public override void Start(LobbyMusicPlayer lobbyMusicPlayer, CancellationToken cancellationToken = default)
    {
        Keyboard.OnKeyCombination += OnKeyCombination;

        if (StereoPlugin.LobbyMusicPlayer != null)
        {
            StereoPlugin.LobbyMusicPlayer.SongLoading += OnSongLoading;
            StereoPlugin.LobbyMusicPlayer.SongLoadingCancelled += OnSongLoadingCancelled;
            StereoPlugin.LobbyMusicPlayer.SongLoadingFailed += OnSongLoadingFailed;
            StereoPlugin.LobbyMusicPlayer.SongLoaded += OnSongLoaded;
        }

        ShowHudButton.SettingChanged += (_, _) =>
        {
            if (HudUtils.TryFindMusicButton(out var button))
            {
                button.SetActive(ShowHudButton.Value);
            }
        };

        StereoPlugin.ShowPlayer = ShowMusicPlayerOnStart.Value;
        _collapsedState.Collapsed = CollapseMusicPlayerOnStart.Value;

        if (VisibilityToggleShortcut.Value.KeyCodes.IsEmpty)
        {
            // If the shortcut to show the music player is empty for whatever reason, force the HUD button to be visible
            // to avoid the music player being stuck closed.
            ShowHudButton.Value = true;
        }

        if (HudUtils.TryFindMusicButton(out var hudButton))
        {
            hudButton.GetComponent<PassiveButton>().SelectButton(StereoPlugin.ShowPlayer);
        }
    }

    public override void Dispose()
    {
        _noteSilhouetteSpriteHandle.Dispose();

        Keyboard.OnKeyCombination -= OnKeyCombination;

        if (StereoPlugin.LobbyMusicPlayer != null)
        {
            StereoPlugin.LobbyMusicPlayer.SongLoading -= OnSongLoading;
            StereoPlugin.LobbyMusicPlayer.SongLoadingCancelled += OnSongLoadingCancelled;
            StereoPlugin.LobbyMusicPlayer.SongLoadingFailed += OnSongLoadingFailed;
            StereoPlugin.LobbyMusicPlayer.SongLoaded -= OnSongLoaded;
        }

        GC.SuppressFinalize(this);
    }

    private void OnKeyCombination(IReadOnlyList<KeyCode> keyCodes)
    {
        if (StereoPlugin.LobbyMusicPlayer is not { } lobbyMusicPlayer)
        {
            return;
        }

        if (VisibilityToggleShortcut.Value.Matches(keyCodes))
        {
            StereoPlugin.ShowPlayer = !StereoPlugin.ShowPlayer;

            if (HudUtils.TryFindMusicButton(out var hudButton))
            {
                hudButton.GetComponent<PassiveButton>().SelectButton(StereoPlugin.ShowPlayer);
            }
        }
        else if (PreviousSongShortcut.Value.Matches(keyCodes))
        {
            Coroutines.Start(lobbyMusicPlayer.PlayNextInQueue(-1, 1f));
        }
        else if (NextSongShortcut.Value.Matches(keyCodes))
        {
            Coroutines.Start(lobbyMusicPlayer.PlayNextInQueue(1, 1f));
        }
        else if (ToggleSongPlaybackShortcut.Value.Matches(keyCodes))
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
    }

    private void OnSongLoading(SongInfo songInfo)
        => SetSongNotification(songInfo.Metadata, "Loading");

    private void OnSongLoadingCancelled(SongMetadata _)
    {
        if (StereoPlugin.LobbyMusicPlayer is { } lobbyMusicPlayer &&
            lobbyMusicPlayer.SongLoadResult is not { State: SongLoadState.Loading } &&
            lobbyMusicPlayer.CurrentSongMetadata != null)
        {
            SetSongNotification(lobbyMusicPlayer.CurrentSongMetadata, "Playing", true);
        }
    }

    private void OnSongLoadingFailed(SongInfo songInfo)
        => SetSongNotification(songInfo.Metadata, "Failed to load");

    private void OnSongLoaded(Song _, SongMetadata metadata)
        => SetSongNotification(metadata, "Playing", true);

    private void SetSongNotification(SongMetadata metadata, string actionText, bool onlyUpdate = false)
    {
        switch (ShowSongNotification.Value)
        {
            case ShowNotification.Yes:
            case ShowNotification.OnlyWhenMusicPlayerHidden when !StereoPlugin.ShowPlayer:
            {
                const string id = "Stereo.Song.State";

                var messageText =
                    $"{actionText} <font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{metadata.Title}</font>.";

                if (onlyUpdate)
                {
                    HudUtils.UpdateNotification(id, messageText);
                }
                else
                {
                    HudUtils.AddOrUpdateNotification(id, messageText, _noteSilhouetteSpriteHandle.Resource);
                }

                break;
            }
        }
    }

    public enum ShowNotification
    {
        No,
        Yes,
        OnlyWhenMusicPlayerHidden
    }
}
