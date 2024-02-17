﻿using System;
using Enamel.Components;
using Enamel.Components.TempComponents;
using Enamel.Enums;
using Enamel.Extensions;
using MoonTools.ECS;

namespace Enamel.Spawners;

public class CharacterSpawner(World world) : Manipulator(world)
{
    public Entity SpawnCharacter(CharacterId characterId, int x, int y)
    {
        var spawnedCharacter = characterId switch
        {
            CharacterId.BlueWiz => SpawnBlueWiz(x, y),
            CharacterId.Ember => SpawnEmber(x, y),
            CharacterId.Loam => SpawnLoam(x, y),
            _ => throw new ArgumentOutOfRangeException(nameof(characterId), characterId, null)
        };
        Set(spawnedCharacter, new TextureIndexComponent(characterId.ToCharacterSprite()));
        return spawnedCharacter;
    }

    private Entity SpawnLoam(int x, int y)
    {
        var loam = SpawnBaseWizard(x, y);
        Set(loam, new LearningSpellComponent(SpellId.StepOnce));
        Set(loam, new LearningSpellComponent(SpellId.RockCharge));

        return loam;
    }

    private Entity SpawnEmber(int x, int y)
    {
        var ember = SpawnBaseWizard(x, y);
        Set(ember, new LearningSpellComponent(SpellId.StepOnce));
        Set(ember, new LearningSpellComponent(SpellId.Fireball));

        return ember;
    }

    private Entity SpawnBlueWiz(int x, int y)
    {
        var blueWiz = SpawnBaseWizard(x, y);
        Set(blueWiz, new LearningSpellComponent(SpellId.StepOnce));
        Set(blueWiz, new LearningSpellComponent(SpellId.ArcaneBlock));
        Set(blueWiz, new LearningSpellComponent(SpellId.ArcaneBubble));

        return blueWiz;
    }

    private Entity SpawnBaseWizard(int x, int y)
    {
        var character = World.CreateEntity();

        Set(character, new AnimationSetComponent(AnimationSet.Wizard));
        Set(character, new AnimationStatusComponent(AnimationType.Idle, Constants.DEFAULT_MILLIS_BETWEEN_FRAMES));
        Set(character, new SpriteOriginComponent(-4, 18));
        Set(character, new DrawLayerComponent(DrawLayer.Units));
        Set(character, new GridCoordComponent(x, y));
        Set(character, new ImpassableFlag());
        Set(character, new HealthComponent(1));
        // Just for testing, I think players would normally only get this flag if they've had some effect applied to them by another player
        Set(character, new PushableFlag());
        return character;
    }
}