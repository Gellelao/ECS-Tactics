using System.Collections.Generic;

namespace Enamel;

public static class TextStorage
{
    private static readonly Dictionary<string, int> StringToId = new();
    private static List<string> _idToString = new();

    public static string GetString(int id)
    {
        return _idToString[id];
    }

    public static int GetId(string text)
    {
        if (!StringToId.ContainsKey(text))
        {
            RegisterString(text);
        }

        return StringToId[text];
    }

    private static void RegisterString(string text)
    {
        _idToString.Add(text);
        StringToId.Add(text, _idToString.Count - 1);
    }
}