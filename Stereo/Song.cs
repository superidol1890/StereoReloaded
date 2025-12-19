using Stereo.Providers;
using UnityEngine;

namespace Stereo;

public record Song(ISongProvider Provider, AudioClip AudioClip);