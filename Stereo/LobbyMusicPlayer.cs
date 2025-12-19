using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reactor.Utilities;
using Stereo.Utilities.Extensions;
using UnityEngine;

namespace Stereo;

public class LobbyMusicPlayer
{
    public Song? CurrentSong => _currentSongInfo?.Owner.Song;

    public SongMetadata? CurrentSongMetadata => _currentSongInfo?.Metadata;

    public SongLoadResult? SongLoadResult { get; private set; }

    public float Duration => CurrentSong?.AudioClip == null ? 0 : CurrentSong.AudioClip.length;

    public float PlaybackTime
    {
        get => _dynamicSound?.Player == null ? 0 : _dynamicSound.Player.time;
        set
        {
            if (_dynamicSound?.Player != null)
            {
                _dynamicSound.Player.time = value;
            }
        }
    }

    public List<SongInfo> QueuedSongs { get; } = [];

    public int QueuePosition { get; private set; }

    public bool Paused => _dynamicSound?.Player == null || !_dynamicSound.Player.isPlaying;

    public bool Loop
    {
        get => _loop;
        set
        {
            _loop = value;

            if (_dynamicSound?.Player != null)
            {
                _dynamicSound.Player.loop = _loop;
            }
        }
    }

    public int Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0, 100);
    }

    public event Action<Song, SongMetadata>? SongEnded;
    public event Action<SongInfo>? SongLoading;
    public event Action<SongMetadata>? SongLoadingCancelled;
    public event Action<SongInfo>? SongLoadingFailed;
    public event Action<Song, SongMetadata>? SongLoaded;

    private LoadedSongInfo? _currentSongInfo;
    private bool _loop;
    private int _volume = 50;
    private DynamicSound? _dynamicSound;
    private CancellationTokenSource? _loadSongCancellationTokenSource;

    private const string SoundName = "LobbyMusic";

    public IEnumerator Play()
    {
        if (CurrentSong == null || _dynamicSound?.Player == null)
        {
            yield return PlaySongAtCurrentQueuePosition(0f);
            yield break;
        }

        _dynamicSound.Player.UnPause();
    }

    public void Pause()
    {
        if (_dynamicSound?.Player != null)
        {
            _dynamicSound.Player.Pause();
        }
    }

    public IEnumerator Stop()
    {
        if (_loadSongCancellationTokenSource != null)
        {
            var cancellationTokenSource = _loadSongCancellationTokenSource;
            _loadSongCancellationTokenSource = null;
            cancellationTokenSource.Cancel();
        }

        if (SoundManager.instance != null)
        {
            SoundManager.Instance.StopNamedSound(SoundName);
        }

        var currentSongInfo = _currentSongInfo;

        SongLoadResult = null;
        _currentSongInfo = null;
        _dynamicSound = null;

        yield return currentSongInfo?.Owner.DisposeAsync().AsTask().AsCoroutine();
    }

    public IEnumerator PlayNextInQueue(int increment = 1, float loadDelay = 0f)
        => PlayInQueue(QueuePosition + increment, loadDelay);

    public IEnumerator PlayInQueue(int queuePosition, float loadDelay = 0f)
    {
        QueuePosition = queuePosition;
        return PlaySongAtCurrentQueuePosition(loadDelay);
    }

    private IEnumerator PlaySongAtCurrentQueuePosition(float loadDelay)
    {
        if (QueuedSongs.Count == 0)
        {
            yield break;
        }

        QueuePosition = QueuePosition < 0
            ? QueuedSongs.Count - 1 - Math.Abs(QueuePosition + 1) % QueuedSongs.Count
            : QueuePosition % QueuedSongs.Count;

        var queuePosition = QueuePosition;
        var isCurrentSong = CurrentSongMetadata == QueuedSongs[queuePosition].Metadata;

        var oldSongLoadResult = SongLoadResult;

        var songInfo = QueuedSongs[queuePosition];
        SongLoadResult = isCurrentSong ? null : new SongLoadResult(SongLoadState.Loading, songInfo.Metadata);

        if (oldSongLoadResult is { State: SongLoadState.Loading } songLoadResult)
        {
            _loadSongCancellationTokenSource?.Cancel();
            _loadSongCancellationTokenSource = null;
            SongLoadingCancelled?.Invoke(songLoadResult.Song);
        }

        if (isCurrentSong)
        {
            yield break;
        }

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));

        var previousLoadSongCancellationTokenSource = _loadSongCancellationTokenSource;
        _loadSongCancellationTokenSource = cancellationTokenSource;

        yield return null;

        SongLoading?.Invoke(songInfo);

        yield return new WaitForSeconds(loadDelay);

        var newSongOwnerTask = Task.Run(
            () => songInfo.Provider
                .LoadSong(cancellationToken)
                .AsTask()
                .WaitAsync(TimeSpan.FromSeconds(15), cancellationToken),
            cancellationToken);

        yield return newSongOwnerTask.AsCoroutine();

        if (newSongOwnerTask.IsCanceled)
        {
            yield break;
        }

        if (!newSongOwnerTask.IsCompletedSuccessfully)
        {
            Debug($"Failed to load song \"{songInfo.Metadata.Title}\": {newSongOwnerTask.Exception}");

            if (SongLoadResult is { State: SongLoadState.Loading } result && result.Song == songInfo.Metadata)
            {
                SongLoadResult = new SongLoadResult(SongLoadState.FailedToLoad, songInfo.Metadata);
            }

            SongLoadingFailed?.Invoke(songInfo);
            yield break;
        }

        SongLoadResult = null;

        var previousSongInfo = _currentSongInfo;
        _currentSongInfo = new LoadedSongInfo(songInfo.Metadata, newSongOwnerTask.Result);
        _currentSongInfo.Owner.Song.AudioClip.name = _currentSongInfo.Metadata.Title;

        SongLoaded?.Invoke(_currentSongInfo.Owner.Song, songInfo.Metadata);

        previousLoadSongCancellationTokenSource?.Cancel();

        SoundManager.Instance.PlayDynamicSound(
            SoundName,
            _currentSongInfo.Owner.Song.AudioClip,
            Loop,
            (Action<AudioSource, float>) ((audioSource, _) =>
            {
                if (audioSource == null)
                {
                    return;
                }

                const float silenceTimestamp = 0.1f;
                const float fadeTimestamp = 0.5f;

                var volume = Volume / 100f;
                var position = audioSource.time;
                var duration = audioSource.clip.length;

                audioSource.volume = position < silenceTimestamp || position + silenceTimestamp > duration
                    ? 0
                    : position < fadeTimestamp + silenceTimestamp
                        ? volume * ((position - silenceTimestamp) / fadeTimestamp)
                        : position + fadeTimestamp + silenceTimestamp > duration
                            ? volume * ((duration - silenceTimestamp - position) / fadeTimestamp)
                            : volume;
            }),
            SoundManager.Instance.MusicChannel);

        if (_dynamicSound?.Player == null)
        {
            _dynamicSound = SoundManager.Instance.soundPlayers._items.First(
                    s => s.Name == SoundName && s.TryCast<DynamicSound>() != null)
                .Cast<DynamicSound>();

            _dynamicSound.Player.loop = _loop;
            _dynamicSound.Player.OnPlaybackEnd(OnSongEnded, CancellationToken.None);
        }

        _dynamicSound.Player.time = 0;

        yield return previousSongInfo?.Owner.DisposeAsync().AsTask().AsCoroutine();
    }

    private void OnSongEnded()
    {
        SongEnded?.Invoke(CurrentSong!, CurrentSongMetadata!);

        if (!Loop)
        {
            Coroutines.Start(PlayNextInQueue());
        }
    }
}
