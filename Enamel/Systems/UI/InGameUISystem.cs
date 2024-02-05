using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Extensions;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems.UI;

public class InGameUiSystem : MoonTools.ECS.System
{
    private readonly MenuUtils _menuUtils;

    private Filter PlayerFilter { get; }

    public InGameUiSystem(World world, MenuUtils menuUtils) : base(world)
    {
        _menuUtils = menuUtils;
        PlayerFilter = FilterBuilder.Include<PlayerNumberComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<DeployWizardsMessage>())
        {
            var endTurnButton = _menuUtils.CreateUiEntity(280, 160, 40, 20);
            Set(endTurnButton, new TextureIndexComponent(Sprite.EndTurnButton));
            Set(endTurnButton, new AnimationSetComponent(AnimationSet.EndTurnButton));
            Set(endTurnButton, new ToggleFrameOnMouseHoverComponent(1, true));
            Set(endTurnButton, new ToggleFrameOnMouseDownComponent(2, true));
            Set(endTurnButton, new OnClickComponent(ClickEvent.EndTurn));

            foreach (var playerEntity in PlayerFilter.Entities)
            {
                var playerNumber = Get<PlayerNumberComponent>(playerEntity).PlayerNumber;
                var characterNumber = Get<SelectedCharacterComponent>(playerEntity).Character;
                var portraitSprite = characterNumber.ToPortraitSprite();

                var portrait = _menuUtils.CreateUiEntity(297, (int) playerNumber * 23);
                Set(portrait, new TextureIndexComponent(portraitSprite));
            }
        }
    }
}