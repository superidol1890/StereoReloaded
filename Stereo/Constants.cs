using System.IO;
using Stereo.Utilities.Structures;
using UnityEngine;

namespace Stereo;

public static class Constants
{
    public static class Paths
    {
        public static readonly string Songs = Path.Combine(BepInEx.Paths.GameRootPath, "Songs");
    }

    public static class Shortcuts
    {
        public static readonly KeyboardShortcut ToggleMusicPlayer = new(new[] { KeyCode.M, KeyCode.P });
        public static readonly KeyboardShortcut PreviousSong = new(new[] { KeyCode.Comma });
        public static readonly KeyboardShortcut NextSong = new(new[] { KeyCode.Period });
        public static readonly KeyboardShortcut ToggleSongPlayback = new(new[] { KeyCode.Comma, KeyCode.Period });
    }
}
