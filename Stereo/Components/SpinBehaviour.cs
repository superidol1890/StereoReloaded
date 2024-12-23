using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Stereo.Components;

[RegisterInIl2Cpp]
public class SpinBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    private void Update()
    {
        transform.Rotate(0f, 0f, 100f * Time.deltaTime);
    }
}
