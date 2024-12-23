using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mitochondria.Resources.FFmpeg.Utilities;

namespace Stereo.Providers.Streaming;

public class StreamingSongProvider(Func<ValueTask<Stream>> streamProvider, Func<Stream, ValueTask> dispose) : ISongProvider
{
    public async ValueTask<SongMetadata> LoadSongMetadata(
        string? fallbackTitle = null,
        CancellationToken cancellationToken = default)
    {
        AudioClipUtils.AudioMetadata audioMetadata;
        await using (var stream = await streamProvider.Invoke())
        {
            audioMetadata = await AudioClipUtils.GetAudioMetadataAsync(stream, cancellationToken);
        }

        return new SongMetadata(
            audioMetadata.Title ?? fallbackTitle ?? "UNTITLED",
            (float) audioMetadata.DurationTimestamp
            * audioMetadata.TimeBase.Denominator / audioMetadata.TimeBase.Numerator);
    }

    public async ValueTask<ISongOwner> LoadSong(CancellationToken cancellationToken = default)
    {
        var stream = await streamProvider.Invoke();

        var taskCompletionSource = new TaskCompletionSource();

        var streamingAudioClip = await AudioClipUtils.CreateStreamingAudioClipAsync(
            stream,
            onStreamingStart: () =>
            {
                taskCompletionSource.TrySetResult();
                return ValueTask.CompletedTask;
            },
            onStreamingEnd: () => dispose.Invoke(stream),
            cancellationToken: cancellationToken);

        await taskCompletionSource.Task;

        cancellationToken.ThrowIfCancellationRequested();

        return new StreamingSongOwner(new Song(this, streamingAudioClip.AudioClip), streamingAudioClip);
    }
}
