using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class MenuSystem : MoonTools.ECS.System
{

    public MenuSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<GoToMainMenuMessage>())
        {
            var mainMenu = World.CreateEntity();
            Set(mainMenu, new DrawLayerComponent(DrawLayer.UserInterface));
            Set(mainMenu, new ScreenPositionComponent(0, 0));
            Set(mainMenu, new TextureIndexComponent(Sprite.TitleScreen));
            Set(mainMenu, new DimensionsComponent(500, 500));
        }
    }
}