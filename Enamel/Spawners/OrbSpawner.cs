using Enamel.Components;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Spawners;

public class OrbSpawner(World world) : Manipulator(world)
{
    public void SpawnColourlessOrb(int x, int y)
    {
        var orb = World.CreateEntity();
        Set(orb, new TextureIndexComponent(Sprite.GreyOrb));
        Set(orb, new ScreenPositionComponent(x, y));
        Set(orb, new DimensionsComponent(12, 12));
        Set(orb, new DraggableComponent(x, y));
        Set(orb, new DrawLayerComponent(DrawLayer.UserInterface));
        Set(orb, new ToggleFrameOnMouseHoverComponent(1));
        Set(orb, new AnimationSetComponent(AnimationSet.Orb));
        Set(orb, new OrbTypeComponent(OrbType.Colourless));
    }
    
    public void SpawnBlueOrb(int x, int y)
    {
        var orb = World.CreateEntity();
        Set(orb, new TextureIndexComponent(Sprite.BlueOrb));
        Set(orb, new ScreenPositionComponent(x, y));
        Set(orb, new DimensionsComponent(12, 12));
        Set(orb, new DraggableComponent(x, y));
        Set(orb, new DrawLayerComponent(DrawLayer.UserInterface));
        Set(orb, new ToggleFrameOnMouseHoverComponent(1));
        Set(orb, new AnimationSetComponent(AnimationSet.Orb));
        Set(orb, new OrbTypeComponent(OrbType.Arcane));
    }
}