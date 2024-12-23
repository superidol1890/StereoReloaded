using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Stereo.Utilities;

public static class HudUtils
{
    public static bool TryFindMusicButton([NotNullWhen(true)] out GameObject? hudButton)
    {
        if (_hudButton != null)
        {
            hudButton = _hudButton;
            return true;
        }

        return (hudButton = _hudButton = GameObject.Find("Hud/Buttons/TopRight/MusicButton")) != null;
    }

    public static void AddOrUpdateNotification(
        string id,
        string messageText,
        Sprite? iconSprite = null,
        Color? textColor = null,
        bool resetShowDuration = true)
    {
        if (!HudManager.InstanceExists || UpdateNotification(id, messageText, resetShowDuration))
        {
            return;
        }

        var notifier = HudManager.Instance.Notifier;

        var newMessage = Object.Instantiate(notifier.notificationMessageOrigin, notifier.transform);
        newMessage.transform.localPosition = new Vector3(0f, 0f, -2f);

        newMessage.SetUp(
            messageText,
            iconSprite ?? notifier.settingsChangeSprite,
            textColor ?? notifier.settingsChangeColor,
            (Action) (() => notifier.OnMessageDestroy(newMessage)));

        newMessage.gameObject.AddComponent<CustomNotificationBehaviour>().Id = id;

        notifier.ShiftMessages();
        notifier.AddMessageToQueue(newMessage);
    }

    public static bool UpdateNotification(string id, string messageText, bool resetShowDuration = true)
    {
        if (!HudManager.InstanceExists)
        {
            return false;
        }

        var notifier = HudManager.Instance.Notifier;
        if (notifier.activeMessages.Count <= 0)
        {
            return false;
        }

        var lastMessage = notifier.activeMessages._items[notifier.activeMessages.Count - 1];
        if (lastMessage.TryGetComponent<CustomNotificationBehaviour>(out var customNotificationBehaviour) &&
            customNotificationBehaviour.Id == id)
        {
            lastMessage.UpdateMessage(messageText);

            if (resetShowDuration)
            {
                lastMessage.alphaTimer = lastMessage.showDuration;
            }

            return true;
        }

        return false;
    }

    private static GameObject? _hudButton;

    [RegisterInIl2Cpp]
    private class CustomNotificationBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public string Id { get; set; } = string.Empty;
    }
}
