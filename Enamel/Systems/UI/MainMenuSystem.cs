using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems.UI;

public class MainMenuSystem : MoonTools.ECS.System
{
    private readonly MenuUtils _menuUtils;

    private Filter PlayerFilter { get; }

    public MainMenuSystem(World world, MenuUtils menuUtils) : base(world)
    {
        _menuUtils = menuUtils;
        PlayerFilter = FilterBuilder.Include<PlayerNumberComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<GoToMainMenuMessage>())
        {
            _menuUtils.DestroyExistingUiEntities();
            DestroyAllPlayers();

            var mainMenu = _menuUtils.CreateUiEntity(0, 0);
            Set(mainMenu, new TextureIndexComponent(Sprite.TitleScreen));

            var startButton = _menuUtils.CreateUiEntity(110, 60, 50, 15);
            Set(
                startButton,
                new TextComponent(TextStorage.GetId("Start"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke)
            );
            Set(startButton, new OnClickComponent(ClickEvent.GoToCharacterSelect));

            var optionsButton = _menuUtils.CreateUiEntity(110, 80, 50, 15);
            Set(
                optionsButton,
                new TextComponent(
                    TextStorage.GetId("Options"),
                    Font.Absolute,
                    Microsoft.Xna.Framework.Color.WhiteSmoke
                )
            );
            Set(optionsButton, new OnClickComponent(ClickEvent.OpenOptions));

            var quitButton = _menuUtils.CreateUiEntity(110, 100, 50, 15);
            Set(
                quitButton,
                new TextComponent(TextStorage.GetId("Quit"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke)
            );
            Set(quitButton, new OnClickComponent(ClickEvent.ExitGame));
        }
    }

    private void DestroyAllPlayers()
    {
        foreach (var entity in PlayerFilter.Entities)
        {
            Destroy(entity);
        }
    }
}