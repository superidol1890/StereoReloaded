using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Reactor.Utilities;
using Stereo.Modules.State;
using Stereo.Providers.Streaming;

namespace Stereo.Modules;

public class LoadSongsFolderModule(SettingsState settingsState) : BaseModule
{
    public override void Start(LobbyMusicPlayer lobbyMusicPlayer, CancellationToken cancellationToken = default)
    {
        _ = Task.Run(
            async () =>
            {
                if (!Directory.Exists(Constants.Paths.Songs))
                {
                    Directory.CreateDirectory(Constants.Paths.Songs);
                    return;
                }

                foreach (var filePath in Directory.GetFiles(Constants.Paths.Songs))
                {
                    try
                    {
                        lobbyMusicPlayer.QueuedSongs.Add(
                            await SongInfo.From(
                                new StreamingFileSystemSongProvider(filePath),
                                Path.GetFileNameWithoutExtension(filePath),
                                cancellationToken));

                        if (settingsState.AutoplayOnStart.Value &&
                            lobbyMusicPlayer.CurrentSong == null &&
                            lobbyMusicPlayer.SongLoadResult == null)
                        {
                            Coroutines.Start(lobbyMusicPlayer.Play());
                        }
                    }
                    catch
                    {
                    }
                }
            },
            cancellationToken);
    }
}
