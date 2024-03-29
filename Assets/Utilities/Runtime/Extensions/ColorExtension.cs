﻿using UnityEngine;


public static class ColorExtension
{
    public static Color WithRed(this Color color, float newRed)
        => new Color(newRed, color.g, color.b, color.a);

    public static Color WithGreen(this Color color, float newGreen)
        => new Color(color.r, newGreen, color.b, color.a);

    public static Color WithBlue(this Color color, float newBlue)
        => new Color(color.r, color.g, newBlue, color.a);

    public static Color WithAlpha(this Color color, float newAlpha)
        => new Color(color.r, color.g, color.b, newAlpha);
}