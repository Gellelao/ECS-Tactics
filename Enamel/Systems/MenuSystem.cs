using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class MenuSystem(World world) : MoonTools.ECS.System(world)
{
    private readonly List<Entity> uiEntities = [];

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<GoToMainMenuMessage>())
        {
            DestroyExistingUiEntities();

            var mainMenu = CreateUiEntity(0, 0);
            Set(mainMenu, new TextureIndexComponent(Sprite.TitleScreen));
            Set(mainMenu, new DrawLayerComponent(DrawLayer.UserInterface));

            var startButton = CreateUiEntity(110, 60, 50, 15);
            World.Set(startButton, new TextComponent(TextStorage.GetId("Start"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            World.Set(startButton, new OnClickComponent(ClickEvent.GoToCharacterSelect));

            var optionsButton = CreateUiEntity(110, 80, 50, 15);
            World.Set(optionsButton, new TextComponent(TextStorage.GetId("Options"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            World.Set(optionsButton, new OnClickComponent(ClickEvent.OpenOptions));
            
            var quitButton = CreateUiEntity(110, 100, 50, 15);
            World.Set(quitButton, new TextComponent(TextStorage.GetId("Quit"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            World.Set(quitButton, new OnClickComponent(ClickEvent.ExitGame));
        }

        if (SomeMessage<GoToCharacterSelectMessage>())
        {
            DestroyExistingUiEntities();

            var mainMenu = CreateUiEntity(0, 0);
            Set(mainMenu, new TextureIndexComponent(Sprite.TitleScreen));
            Set(mainMenu, new DrawLayerComponent(DrawLayer.UserInterface));

            var backButton = CreateUiEntity(80, 110, 50, 15);
            World.Set(backButton, new TextComponent(TextStorage.GetId("Back"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            World.Set(backButton, new OnClickComponent(ClickEvent.GoToMainMenu));
        }
    }

    private void DestroyExistingUiEntities(){
        foreach(Entity entity in uiEntities){
            Destroy(entity);
        }
        uiEntities.Clear();
    }

    private Entity CreateUiEntity(int x, int y, int width, int height){
        var entity = CreateUiEntity(x, y);
        Set(entity, new DimensionsComponent(width, height));
        return entity;
    }

    private Entity CreateUiEntity(int x, int y){
        var entity = World.CreateEntity();
        Set(entity, new DrawLayerComponent(DrawLayer.UserInterface));
        Set(entity, new ScreenPositionComponent(x, y));

        uiEntities.Add(entity);

        return entity;
    }
}