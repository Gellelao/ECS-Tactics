using System;
using Enamel.Enums;

namespace Enamel.Extensions;

public static class CharacterExtensions
{
    public static Sprite ToCharacterSprite(this Character character)
    {
        return character switch
        {
            Character.BlueWiz => Sprite.BlueWizard,
            Character.Ember => Sprite.EmberWizard,
            Character.Loam => Sprite.LoamWizard,
            _ => throw new ArgumentOutOfRangeException(nameof(character), character, null)
        };
    }

    public static Sprite ToPortraitSprite(this Character character)
    {
        return character switch
        {
            Character.BlueWiz => Sprite.BlueWizPortrait,
            Character.Ember => Sprite.EmberWizPortrait,
            Character.Loam => Sprite.LoamWizPortrait,
            _ => throw new ArgumentOutOfRangeException(nameof(character), character, null)
        };
    }
}