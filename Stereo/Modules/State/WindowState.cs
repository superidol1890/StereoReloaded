using Reactor.Utilities.ImGui;

namespace Stereo.Modules.State;

public class WindowState : BaseModule
{
    public Window? SubWindow { get; set; }

    public void ToggleWindow(Window window)
        => SubWindow = SubWindow == window ? null : window;

    public override void OnGUI(LobbyMusicPlayer lobbyMusicPlayer)
        => SubWindow?.OnGUI();
}
