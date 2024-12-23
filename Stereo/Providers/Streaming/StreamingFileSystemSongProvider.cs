using System.Threading;
using System.Threading.Tasks;
using Mitochondria.Resources.FFmpeg.Utilities;

namespace Stereo.Providers.Streaming;

public class StreamingFileSystemSongProvider(string filePath) : ISongProvider
{
    public string FilePath { get; } = filePath;

    public async ValueTask<SongMetadata> LoadSongMetadata(
        string? fallbackTitle = null,
        CancellationToken cancellationToken = default)
    {
        var audioMetadata = await AudioClipUtils.GetAudioMetadataAsync(FilePath, cancellationToken);

        return new SongMetadata(
            audioMetadata.Title ?? fallbackTitle ?? "UNTITLED",
            (float) audioMetadata.DurationTimestamp
            * audioMetadata.TimeBase.Denominator / audioMetadata.TimeBase.Numerator);
    }

    public async ValueTask<ISongOwner> LoadSong(CancellationToken cancellationToken = default)
    {
        var streamingAudioClip =
            await AudioClipUtils.CreateStreamingAudioClipAsync(FilePath, cancellationToken: cancellationToken);

        return new StreamingSongOwner(new Song(this, streamingAudioClip.AudioClip), streamingAudioClip);
    }
}
