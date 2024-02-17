using System;
using Enamel.Enums;

namespace Enamel.Extensions;

public static class CharacterExtensions
{
    public static Sprite ToCharacterSprite(this CharacterId characterId)
    {
        return characterId switch
        {
            CharacterId.BlueWiz => Sprite.BlueWizard,
            CharacterId.Ember => Sprite.EmberWizard,
            CharacterId.Loam => Sprite.LoamWizard,
            _ => throw new ArgumentOutOfRangeException(nameof(characterId), characterId, null)
        };
    }

    public static Sprite ToPortraitSprite(this CharacterId characterId)
    {
        return characterId switch
        {
            CharacterId.BlueWiz => Sprite.BlueWizPortrait,
            CharacterId.Ember => Sprite.EmberWizPortrait,
            CharacterId.Loam => Sprite.LoamWizPortrait,
            _ => throw new ArgumentOutOfRangeException(nameof(characterId), characterId, null)
        };
    }
}