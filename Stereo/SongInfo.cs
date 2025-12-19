using System.Threading;
using System.Threading.Tasks;
using Stereo.Providers;

namespace Stereo;

public record SongInfo(SongMetadata Metadata, ISongProvider Provider)
{
    public static async ValueTask<SongInfo> From(
        ISongProvider provider,
        string? fallbackTitle = null,
        CancellationToken cancellationToken = default)
        => new(await provider.LoadSongMetadata(fallbackTitle, cancellationToken), provider);
}