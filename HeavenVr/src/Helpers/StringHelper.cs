using System;

namespace HeavenVr.Helpers;

public static class StringHelper
{
    public static bool ContainsCaseInsensitive(string baseText, string textPart)
    {
        return baseText.IndexOf(textPart, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}