using UnityEngine;

namespace Stereo.Utilities.Extensions;

public static class GuiExtensions
{
    public static int CalcFontSize(this GUIStyle style, GUIContent content, int maxFontSize, float width, float height)
    {
        var tempTitleStyle = new GUIStyle(style)
        {
            fontSize = maxFontSize + 1,
            wordWrap = true
        };

        float requiredHeight;

        do
        {
            tempTitleStyle.fontSize--;
            requiredHeight = tempTitleStyle.CalcHeight(content, width);
        } while (tempTitleStyle.fontSize > 0 && requiredHeight > height);

        return tempTitleStyle.fontSize;
    }
}
