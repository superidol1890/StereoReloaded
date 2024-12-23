using System;
using System.Collections;
using Reactor.Utilities;
using Stereo.Utilities.Structures;
using UnityEngine;

namespace Stereo.Utilities.Extensions;

public static class GUIUtils
{
    public static void KeyboardShortcut(
        ref bool captureKeyboard,
        ref KeyboardShortcut? tempKeyboardShortcut,
        KeyboardShortcut keyboardShortcut,
        Action<KeyboardShortcut> setKeyboardShortcut,
        KeyboardShortcut defaultShortcut,
        string text,
        int maxKeys = 5)
    {
        var guiEnabled = GUI.enabled;

        var labelStyle = guiEnabled
            ? GUI.skin.label
            : new GUIStyle(GUI.skin.label)
            {
                normal = new GUIStyleState
                {
                    textColor = Color.gray
                }
            };

        var oldCaptureKeyboard = captureKeyboard;

        if (guiEnabled && captureKeyboard && Event.current.isKey)
        {
            if (Event.current.type == EventType.keyDown)
            {
                var keyCode = Event.current.keyCode;
                tempKeyboardShortcut ??= Structures.KeyboardShortcut.Empty;

                if (keyCode is not KeyCode.None && !tempKeyboardShortcut.KeyCodes.Contains(keyCode))
                {
                    tempKeyboardShortcut =
                        new KeyboardShortcut(tempKeyboardShortcut.KeyCodes.Add(Event.current.keyCode));

                    if (tempKeyboardShortcut.KeyCodes.Length == maxKeys)
                    {
                        captureKeyboard = false;
                    }
                }
            }
            else if (Event.current.type == EventType.keyUp)
            {
                captureKeyboard = false;
            }
        }

        if (oldCaptureKeyboard && !captureKeyboard && tempKeyboardShortcut != null)
        {
            var newKeyboardShortcut = tempKeyboardShortcut;
            tempKeyboardShortcut = null;

            IEnumerator CoUpdateKeyboardShortcut()
            {
                yield return null;
                setKeyboardShortcut.Invoke(newKeyboardShortcut);
            }

            setKeyboardShortcut.Invoke(new KeyboardShortcut(newKeyboardShortcut.KeyCodes.Add(KeyCode.None)));
            Coroutines.Start(CoUpdateKeyboardShortcut());
        }

        GUILayout.Space(2f);

        GUILayout.BeginVertical(new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset
            {
                top = 5,
                bottom = 5,
                left = 10,
                right = 5
            }
        });
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(text, labelStyle);

                if (captureKeyboard)
                {
                    if (GUILayout.Button("Cancel", GUILayout.Width(50f)))
                    {
                        captureKeyboard = false;
                        tempKeyboardShortcut = null;
                    }
                }
                else
                {
                    if (GUILayout.Button("Edit", GUILayout.Width(50f)))
                    {
                        captureKeyboard = true;
                        tempKeyboardShortcut = Structures.KeyboardShortcut.Empty;
                    }
                }

                GUI.enabled = guiEnabled && !captureKeyboard && !keyboardShortcut.KeyCodes.IsEmpty;
                {
                    if (GUILayout.Button("Clear", GUILayout.Width(50f)))
                    {
                        setKeyboardShortcut.Invoke(Structures.KeyboardShortcut.Empty);
                    }
                }
                GUI.enabled = guiEnabled;

                GUI.enabled = guiEnabled &&
                              !captureKeyboard &&
                              !keyboardShortcut.Matches(defaultShortcut.KeyCodes, [KeyCode.None]);
                {
                    if (GUILayout.Button("Reset", GUILayout.Width(50f)))
                    {
                        setKeyboardShortcut.Invoke(defaultShortcut);
                    }
                }
                GUI.enabled = guiEnabled;

                if (captureKeyboard &&
                    (Math.Pow(Input.GetAxis("Mouse X"), 2f) > 0.2f || Math.Pow(Input.GetAxis("Mouse Y"), 2f) > 0.2f))
                {
                    captureKeyboard = false;
                    tempKeyboardShortcut = null;
                }
            }
            GUILayout.EndHorizontal();

            var shortcutText = captureKeyboard && tempKeyboardShortcut != null
                ? tempKeyboardShortcut.KeyCodes.IsEmpty
                    ? "MOVE THE MOUSE TO CANCEL"
                    : tempKeyboardShortcut.ToString()
                : keyboardShortcut.KeyCodes.IsEmpty
                    ? "NONE"
                    : keyboardShortcut.ToString();

            GUILayout.Label(shortcutText, labelStyle);
        }
        GUILayout.EndVertical();

        GUILayout.Space(2f);
    }

    public static TEnum ToggleGroup<TEnum>(TEnum value, Func<TEnum, string> getOptionText, string text)
        where TEnum : struct, Enum
    {
        var guiEnabled = GUI.enabled;

        var labelStyle = guiEnabled
            ? GUI.skin.label
            : new GUIStyle(GUI.skin.label)
            {
                normal = new GUIStyleState
                {
                    textColor = Color.gray
                }
            };

        var newValue = value;

        GUILayout.Space(2f);

        GUILayout.BeginVertical(new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset
            {
                top = 5,
                bottom = 10,
                left = 10,
                right = 10
            }
        });
        {
            GUILayout.Label(text, labelStyle);

            foreach (var @enum in Enum.GetValues<TEnum>())
            {
                var isSelected = @enum.Equals(value);
                if (GUILayout.Toggle(isSelected, getOptionText.Invoke(@enum)) && !isSelected)
                {
                    newValue = @enum;
                }
            }
        }
        GUILayout.EndVertical();

        GUILayout.Space(2f);

        return newValue;
    }
}
