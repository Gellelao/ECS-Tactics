using System;
using System.Text.RegularExpressions;
using Enamel.Enums;

namespace Enamel.Extensions;

public static class CharacterExtensions
{
    public static Sprite ToSprite(this Character character)
    {
        return character switch
        {
            Character.BlueWiz => Sprite.BlueWizard,
            _ => throw new ArgumentOutOfRangeException(nameof(character), character, null)
        };
    }
}