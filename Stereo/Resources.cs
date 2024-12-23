using System.Reflection;
using Mitochondria.Resources;
using Mitochondria.Resources.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stereo;

public static class Resources
{
    private static readonly Assembly Assembly = typeof(StereoPlugin).Assembly;

    private const string AssemblyName = "Stereo";

    private static Texture2D LoadTexture(string resourceName)
        => TextureUtils.CreateTexture(Assembly.GetManifestResourceStream(resourceName)!);

    private static ResourceHandle<Sprite>.ResourceProvider CreateSpriteResourceProvider(
        ResourceHandle<Texture2D>.ResourceProvider textureProvider,
        float pixelsPerUnit)
    {
        var handle = textureProvider.AcquireHandle();

        return new ResourceHandle<Sprite>.ResourceProvider(
            () => SpriteUtils.CreateSprite(textureProvider.AcquireHandle().Resource, pixelsPerUnit).DontDestroy(),
            node =>
            {
                if (node.Loaded && node.Resource != null)
                {
                    node.Resource.Destroy();
                    handle.Dispose();
                }
            });
    }

    private static ResourceHandle<Texture2D>.ResourceProvider CreateTextureResourceProvider(string resourceName)
        => new(
            () => LoadTexture(resourceName).DontDestroy(),
            node =>
            {
                if (node.Loaded && node.Resource != null)
                {
                    node.Resource.Destroy();
                }
            });

    public static class LobbyMusicPlayer
    {
        public static ResourceHandle<Texture2D>.ResourceProvider NoteSilhouetteTextureProvider { get; }

        public static ResourceHandle<Sprite>.ResourceProvider NoteSilhouetteSpriteProvider { get; }

        static LobbyMusicPlayer()
        {
            NoteSilhouetteTextureProvider =
                CreateTextureResourceProvider($"{AssemblyName}.Resources.NoteSilhouette.png");

            NoteSilhouetteSpriteProvider = CreateSpriteResourceProvider(NoteSilhouetteTextureProvider, 100f);
        }

        public static class Buttons
        {
            public static ResourceHandle<Texture2D>.ResourceProvider PlayTextureProvider { get; }

            public static ResourceHandle<Texture2D>.ResourceProvider PauseTextureProvider { get; }

            public static ResourceHandle<Texture2D>.ResourceProvider ReloadTextureProvider { get; }

            public static ResourceHandle<Texture2D>.ResourceProvider PreviousTextureProvider { get; }

            public static ResourceHandle<Texture2D>.ResourceProvider NextTextureProvider { get; }

            public static ResourceHandle<Texture2D>.ResourceProvider QueueTextureProvider { get; }

            public static ResourceHandle<Texture2D>.ResourceProvider WrenchTextureProvider { get; }

            static Buttons()
            {
                PlayTextureProvider = CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Play.png");
                PauseTextureProvider = CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Pause.png");
                ReloadTextureProvider = CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Reload.png");

                PreviousTextureProvider =
                    CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Previous.png");

                NextTextureProvider = CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Next.png");
                QueueTextureProvider = CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Queue.png");
                WrenchTextureProvider = CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Wrench.png");
            }

            public static class Arrow
            {
                public static ResourceHandle<Texture2D>.ResourceProvider UpTextureProvider { get; }

                public static ResourceHandle<Texture2D>.ResourceProvider DownTextureProvider { get; }

                static Arrow()
                {
                    UpTextureProvider = CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Arrow.Up.png");

                    DownTextureProvider =
                        CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Arrow.Down.png");
                }
            }

            public static class Note
            {
                public static ResourceHandle<Texture2D>.ResourceProvider InactiveTextureProvider { get; }

                public static ResourceHandle<Texture2D>.ResourceProvider ActiveTextureProvider { get; }

                public static ResourceHandle<Texture2D>.ResourceProvider SelectedTextureProvider { get; }

                public static ResourceHandle<Sprite>.ResourceProvider InactiveSpriteProvider { get; }

                public static ResourceHandle<Sprite>.ResourceProvider ActiveSpriteProvider { get; }

                public static ResourceHandle<Sprite>.ResourceProvider SelectedSpriteProvider { get; }

                static Note()
                {
                    InactiveTextureProvider =
                        CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Note.Inactive.png");

                    ActiveTextureProvider =
                        CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Note.Active.png");

                    SelectedTextureProvider =
                        CreateTextureResourceProvider($"{AssemblyName}.Resources.Button.Note.Selected.png");

                    InactiveSpriteProvider = CreateSpriteResourceProvider(InactiveTextureProvider, 100f);
                    ActiveSpriteProvider = CreateSpriteResourceProvider(ActiveTextureProvider, 100f);
                    SelectedSpriteProvider = CreateSpriteResourceProvider(SelectedTextureProvider, 100f);
                }
            }
        }
    }
}
