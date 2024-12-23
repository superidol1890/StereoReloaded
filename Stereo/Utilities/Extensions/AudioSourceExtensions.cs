using System;
using System.Collections;
using System.Threading;
using Reactor.Utilities;
using UnityEngine;

namespace Stereo.Utilities.Extensions;

public static class AudioSourceExtensions
{
    /// <remarks>
    /// Checks if playback got to the end using a very crude mechanism. Also triggers when the audio source is set to
    /// loop. May trigger incorrectly if playback is about to finish and the position is moved back. May also not
    /// trigger under very specific circumstances, such as the audio clip being too short or frame updates being too
    /// slow.
    /// </remarks>
    public static void OnPlaybackEnd(
        this AudioSource audioSource,
        Action callback,
        CancellationToken cancellationToken = default)
    {
        Coroutines.Start(CoOnPlaybackEnd(audioSource, callback, cancellationToken));
    }

    private static IEnumerator CoOnPlaybackEnd(
        this AudioSource audioSource,
        Action callback,
        CancellationToken cancellationToken = default)
    {
        // If the last recorded playback position is more than the current position then the audio clip must have reset.
        var lastTime = 0f;
        var audioClip = audioSource.clip;

        while (true)
        {
            if (cancellationToken.IsCancellationRequested || audioSource == null)
            {
                yield break;
            }

            // 0.5s is some arbitrary number, but the premise is that if the playback position is moved back most of the
            // length of the audio, it was probably reset and thus, is either looping or ended.
            var probablyEnded = audioSource.clip != null &&
                                audioSource.clip == audioClip &&
                                lastTime > audioSource.time &&
                                lastTime - audioSource.time > audioSource.clip.length - 0.5f;

            lastTime = audioSource.time;
            audioClip = audioSource.clip;

            if (probablyEnded)
            {
                callback.Invoke();
            }

            yield return null;
        }
    }
}
