using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Stereo.Providers.Memory;

public class MemorySongProvider(Func<AudioClip> audioClipProvider) : ISongProvider
{
    public Func<AudioClip> AudioClipProvider { get; } = audioClipProvider;

    public ValueTask<SongMetadata> LoadSongMetadata(
        string? fallbackTitle = null,
        CancellationToken cancellationToken = default)
    {
        var audioClip = AudioClipProvider.Invoke();
        return ValueTask.FromResult(new SongMetadata(audioClip.name, audioClip.length));
    }

    public ValueTask<ISongOwner> LoadSong(CancellationToken _)
        => ValueTask.FromResult((ISongOwner) new MemorySongOwner(new Song(this, AudioClipProvider.Invoke())));
}
