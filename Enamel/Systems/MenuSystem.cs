using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class MenuSystem : MoonTools.ECS.System
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    public MenuSystem(World world, int screenWidth, int screenHeight) : base(world)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<GoToMainMenuMessage>())
        {
            var mainMenu = World.CreateEntity();
            Set(mainMenu, new DrawLayerComponent(DrawLayer.UserInterface));
            Set(mainMenu, new ScreenPositionComponent(0, 0));
            Set(mainMenu, new TextureIndexComponent(Sprite.GreenRectangle));
            Set(mainMenu, new DimensionsComponent(500, 500));
        }
    }
}