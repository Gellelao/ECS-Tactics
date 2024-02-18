using System;
using System.Collections.Generic;
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
    private const int PORTRAIT_X = 296;
    private const int PORTRAIT_HEIGHT = 23;
    private readonly MenuUtils _menuUtils;
    private readonly Dictionary<PlayerId, Entity> _portraitsByPlayer;
    private int _numberOfPlayers;

    private Filter PlayerFilter { get; }

    public InGameUiSystem(World world, MenuUtils menuUtils) : base(world)
    {
        _menuUtils = menuUtils;
        _portraitsByPlayer = new Dictionary<PlayerId, Entity>();
        PlayerFilter = FilterBuilder.Include<PlayerIdComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<DeployWizardsMessage>())
        {
            _numberOfPlayers = PlayerFilter.Count;
            var endTurnButton = _menuUtils.CreateUiEntity(280, 160, 40, 20);
            Set(endTurnButton, new TextureIndexComponent(Sprite.EndTurnButton));
            Set(endTurnButton, new AnimationSetComponent(AnimationSet.EndTurnButton));
            Set(endTurnButton, new ToggleFrameOnMouseHoverComponent(1));
            Set(endTurnButton, new ToggleFrameOnMouseDownComponent(2));
            Set(endTurnButton, new OnClickComponent(ClickEvent.EndTurn));
            
            var testOrb = World.CreateEntity();
            World.Set(testOrb, new TextureIndexComponent(Sprite.BlueOrb));
            World.Set(testOrb, new ScreenPositionComponent(110, 110));
            World.Set(testOrb, new DimensionsComponent(12, 12));
            World.Set(testOrb, new DraggableComponent(30, 120));
            World.Set(testOrb, new DrawLayerComponent(DrawLayer.LitOverlay));
            World.Set(testOrb, new ToggleFrameOnMouseHoverComponent(1));
            World.Set(testOrb, new ToggleFrameOnMouseDownComponent(2));
            World.Set(testOrb, new AnimationSetComponent(AnimationSet.Orb));
            World.Set(testOrb, new OrbTypeComponent(OrbType.Solar));
            
            var testOrb1 = World.CreateEntity();
            World.Set(testOrb1, new TextureIndexComponent(Sprite.BlueOrb));
            World.Set(testOrb1, new ScreenPositionComponent(100, 100));
            World.Set(testOrb1, new DimensionsComponent(12, 12));
            World.Set(testOrb1, new DraggableComponent(30, 80));
            World.Set(testOrb1, new DrawLayerComponent(DrawLayer.LitOverlay));
            World.Set(testOrb1, new ToggleFrameOnMouseHoverComponent(1));
            World.Set(testOrb1, new ToggleFrameOnMouseDownComponent(2));
            World.Set(testOrb1, new AnimationSetComponent(AnimationSet.Orb));
            World.Set(testOrb1, new OrbTypeComponent(OrbType.Arcane));

            _portraitsByPlayer.Clear();
            foreach (var playerEntity in PlayerFilter.Entities)
            {
                var playerId = Get<PlayerIdComponent>(playerEntity).PlayerId;
                var characterId = Get<SelectedCharacterComponent>(playerEntity).CharacterId;
                var portraitSprite = characterId.ToPortraitSprite();

                var portrait = _menuUtils.CreateUiEntity(
                    PORTRAIT_X,
                    (int) playerId * PORTRAIT_HEIGHT + 1 + (int) playerId
                );
                Set(portrait, new TextureIndexComponent(portraitSprite));

                _portraitsByPlayer.Add(playerId, portrait);
            }

            var selectedPortraitFrame = _menuUtils.CreateUiEntity(295, 0);
            Set(selectedPortraitFrame, new TextureIndexComponent(Sprite.SelectedPortrait));
            Set(selectedPortraitFrame, new DrawLayerComponent(DrawLayer.UserInterfaceOverlay));

            OrderPortraits();
        }

        if (SomeMessage<EndTurnMessage>())
        {
            OrderPortraits();
        }
    }

    private void OrderPortraits()
    {
        var currentPlayerId = Get<PlayerIdComponent>(GetSingletonEntity<CurrentPlayerFlag>()).PlayerId;
        for (var i = 0; i < _numberOfPlayers; i++)
        {
            var portrait = _portraitsByPlayer[currentPlayerId];
            var speed = i == _numberOfPlayers - 1 ? 200 : 80;
            Set(portrait, new MovingToScreenPositionComponent(PORTRAIT_X, i * PORTRAIT_HEIGHT + 1 + i, speed));
            currentPlayerId = Utils.Utils.GetNextPlayer(currentPlayerId, _numberOfPlayers);
        }
    }
}