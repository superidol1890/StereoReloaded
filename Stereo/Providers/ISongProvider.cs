using System.Threading;
using System.Threading.Tasks;

namespace Stereo.Providers;

public interface ISongProvider
{
    public ValueTask<SongMetadata> LoadSongMetadata(
        string? fallbackTitle = null,
        CancellationToken cancellationToken = default);

    public ValueTask<ISongOwner> LoadSong(CancellationToken cancellationToken = default);
}
