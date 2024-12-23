using System;
using System.Threading;

namespace Stereo.Modules;

public abstract class BaseModule : IDisposable
{
    public virtual void Start(LobbyMusicPlayer lobbyMusicPlayer, CancellationToken cancellationToken = default)
    {
    }

    public virtual void OnGUI(LobbyMusicPlayer lobbyMusicPlayer)
    {
    }

    public virtual void OnMusicPlayerGUI(LobbyMusicPlayer lobbyMusicPlayer)
    {
    }

    public virtual void Dispose()
        => GC.SuppressFinalize(this);
}
