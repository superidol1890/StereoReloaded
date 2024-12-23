using System;
using BepInEx.Configuration;

namespace Stereo.Utilities.Structures;

public static class KeyboardShortcutConverterFactory
{
    public static TypeConverter Create()
        => new()
        {
            ConvertToObject = ConvertToObject,
            ConvertToString = ConvertToString
        };

    private static object ConvertToObject(string data, Type type)
    {
        if (!typeof(KeyboardShortcut).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Cannot convert type \"{type.Name}\" to type \"{nameof(KeyboardShortcut)}\"");
        }

        if (!KeyboardShortcut.TryParse(data, out var keyboardShortcut))
        {
            throw new ArgumentException($"Cannot convert data to type \"{nameof(KeyboardShortcut)}\"");
        }

        return keyboardShortcut;
    }

    private static string ConvertToString(object obj, Type type)
    {
        if (obj is not KeyboardShortcut keyboardShortcut)
        {
            throw new ArgumentException(
                $"Cannot convert type \"{type.Name}\" to a string since it is not of type \"{nameof(KeyboardShortcut)}\"");
        }

        return keyboardShortcut.Serialize();
    }
}
