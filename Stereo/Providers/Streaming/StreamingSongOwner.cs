using System;
using System.Threading.Tasks;
using Mitochondria.Resources.FFmpeg.Utilities;

namespace Stereo.Providers.Streaming;

public class StreamingSongOwner(Song streamingSong, StreamingAudioClip streamingAudioClip) : ISongOwner
{
    public Song Song { get; } = streamingSong;

    public StreamingAudioClip StreamingAudioClip { get; } = streamingAudioClip;

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return StreamingAudioClip.DisposeAsync();
    }
}
