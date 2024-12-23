using System;
using System.Threading.Tasks;

namespace Stereo.Providers.Memory;

public class MemorySongOwner(Song song) : ISongOwner
{
    public Song Song { get; } = song;

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
