using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Stereo.Utilities.Structures;

public record KeyboardShortcut(ImmutableArray<KeyCode> KeyCodes)
{
    public static readonly KeyboardShortcut Empty = new(ImmutableArray<KeyCode>.Empty);

    public static bool TryParse(string data, [NotNullWhen(true)] out KeyboardShortcut? keyboardShortcut)
    {
        var keyCodes = new List<KeyCode>();

        var keyCodesStrings = data.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var keyCodeString in keyCodesStrings)
        {
            if (int.TryParse(keyCodeString, out var keyCode))
            {
                keyCodes.Add((KeyCode) keyCode);
            }
            else
            {
                keyboardShortcut = default;
                return false;
            }
        }

        keyboardShortcut = new KeyboardShortcut(keyCodes.ToImmutableArray());
        return true;
    }

    public KeyboardShortcut(KeyboardShortcut original)
    {
        KeyCodes = original.KeyCodes;
    }

    public KeyboardShortcut(IEnumerable<KeyCode> keyCodes) : this(keyCodes.ToImmutableArray())
    {
    }

    public bool Matches(IReadOnlyList<KeyCode> keyCodes, IReadOnlyList<KeyCode>? ignoredKeyCodes = null)
    {
        if (KeyCodes.Length == 0)
        {
            return false;
        }

        if (ignoredKeyCodes == null)
        {
            return KeyCodes.Length == keyCodes.Count && KeyCodes.All(keyCodes.Contains);
        }

        var ignoreKeyCodesArray = ignoredKeyCodes.ToArray();
        var thisKeyCodesFiltered = KeyCodes.Except(ignoreKeyCodesArray).ToArray();
        var keyCodesFiltered = keyCodes.Except(ignoreKeyCodesArray).ToArray();

        return thisKeyCodesFiltered.Length == keyCodesFiltered.Length &&
               thisKeyCodesFiltered.All(keyCodesFiltered.Contains);
    }

    public string Serialize()
        => string.Join(" + ", KeyCodes.Cast<int>());

    public sealed override string ToString()
        => string.Join(" + ", KeyCodes.Where(keyCode => keyCode is not KeyCode.None));

    public virtual bool Equals(KeyboardShortcut? other)
        => other != null && KeyCodes.SequenceEqual(other.KeyCodes);

    public override int GetHashCode()
        => KeyCodes.GetHashCode();
}
