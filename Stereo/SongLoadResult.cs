namespace Stereo;

public readonly record struct SongLoadResult(SongLoadState State, SongMetadata Song);

public enum SongLoadState
{
    Loading,
    FailedToLoad
}
