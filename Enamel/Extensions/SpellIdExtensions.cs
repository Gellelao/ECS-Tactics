using System;
using System.Text.RegularExpressions;
using Enamel.Enums;

namespace Enamel.Extensions;

public static class SpellIdExtensions
{
    public static string ToName(this SpellId spellId)
    {
        var defaultName = Enum.GetName(typeof(SpellId), spellId);
        return Regex.Replace(defaultName, "(\\B[A-Z])", " $1");
    }
}