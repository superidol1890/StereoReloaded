using System;

namespace Stereo;

public interface ISongOwner : IAsyncDisposable
{
    public Song Song { get; }
}
