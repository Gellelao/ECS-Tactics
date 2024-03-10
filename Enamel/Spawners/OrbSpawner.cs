using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Spawners;

public class OrbSpawner(World world) : Manipulator(world)
{
    public void SpawnColourlessOrb(int x, int y)
    {
        var orb = CreateMinimalOrb(Sprite.GreyOrb, OrbType.Colourless);
        MaterializeOrb(orb, x, y);
    }
    
    public void SpawnBlueOrb(int x, int y)
    {
        var orb = CreateMinimalOrb(Sprite.BlueOrb, OrbType.Arcane);
        MaterializeOrb(orb, x, y);
    }

    public void MaterializeOrb(Entity orb, int x, int y)
    {
        Set(orb, new ScreenPositionComponent(Constants.HAND_X, Constants.HAND_Y_START));
        Set(orb, new MovingToScreenPositionComponent(x, y, Constants.HAND_ORB_SPEED));
        Set(orb, new DimensionsComponent(12, 12));
        Set(orb, new DraggableComponent(x, y));
        Set(orb, new DrawLayerComponent(DrawLayer.UserInterface));
        Set(orb, new ToggleFrameOnMouseHoverComponent(1));
        Set(orb, new AnimationSetComponent(AnimationSet.Orb));
    }

    public void DematerializeOrb(Entity orb)
    {
        Remove<ScreenPositionComponent>(orb);
        Remove<DimensionsComponent>(orb);
        Remove<DraggableComponent>(orb);
        Remove<DrawLayerComponent>(orb);
        Remove<ToggleFrameOnMouseHoverComponent>(orb);
        Remove<AnimationSetComponent>(orb);
    }

    public void AddCharacterOrbsToPlayerBag(Entity player, CharacterId character)
    {
        var orbs = new List<Entity>();
        switch (character)
        {
            case CharacterId.BlueWiz:
                orbs.Add(CreateMinimalOrb(Sprite.BlueOrb, OrbType.Arcane));
                orbs.Add(CreateMinimalOrb(Sprite.BlueOrb, OrbType.Arcane));
                orbs.Add(CreateMinimalOrb(Sprite.BlueOrb, OrbType.Arcane));
                orbs.Add(CreateMinimalOrb(Sprite.GreyOrb, OrbType.Colourless));
                orbs.Add(CreateMinimalOrb(Sprite.GreyOrb, OrbType.Colourless));
                orbs.Add(CreateMinimalOrb(Sprite.GreyOrb, OrbType.Colourless));
                orbs.Add(CreateMinimalOrb(Sprite.GreyOrb, OrbType.Colourless));
                orbs.Add(CreateMinimalOrb(Sprite.GreyOrb, OrbType.Colourless));
                orbs.Add(CreateMinimalOrb(Sprite.GreyOrb, OrbType.Colourless));
                break;
            case CharacterId.Ember:
                break;
            case CharacterId.Loam:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(character), character, null);
        }

        foreach (var orb in orbs)
        {
            Relate(player, orb, new OrbInBagRelation());
        }
    }

    private Entity CreateMinimalOrb(Sprite sprite, OrbType orbType)
    {
        var orb = CreateEntity();
        Set(orb, new TextureIndexComponent(sprite));
        Set(orb, new OrbTypeComponent(orbType));
    }
}