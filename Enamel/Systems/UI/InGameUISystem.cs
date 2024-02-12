﻿using System;
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
    private readonly Dictionary<Player, Entity> _portraitsByPlayer;
    private int _numberOfPlayers;

    private Filter PlayerFilter { get; }

    public InGameUiSystem(World world, MenuUtils menuUtils) : base(world)
    {
        _menuUtils = menuUtils;
        _portraitsByPlayer = new Dictionary<Player, Entity>();
        PlayerFilter = FilterBuilder.Include<PlayerNumberComponent>().Build();
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
            
            var tome = World.CreateEntity();
            World.Set(tome, new TextureIndexComponent(Sprite.Tome));
            World.Set(tome, new ScreenPositionComponent(30, 120));
            World.Set(tome, new DrawLayerComponent(DrawLayer.UserInterface));

            var testOrb = World.CreateEntity();
            World.Set(testOrb, new TextureIndexComponent(Sprite.BlueOrb));
            World.Set(testOrb, new ScreenPositionComponent(100, 100));
            World.Set(testOrb, new DimensionsComponent(12, 12));
            World.Set(testOrb, new DraggableComponent(30, 80));
            World.Set(testOrb, new DrawLayerComponent(DrawLayer.UserInterfaceOverlay)); // Uargh!
            World.Set(testOrb, new ToggleFrameOnMouseHoverComponent(1));
            World.Set(testOrb, new ToggleFrameOnMouseDownComponent(2));
            World.Set(testOrb, new AnimationSetComponent(AnimationSet.Orb));

            var testSocket = World.CreateEntity();
            World.Set(testSocket, new TextureIndexComponent(Sprite.Socket));
            World.Set(testSocket, new ScreenPositionComponent(70, 130));
            World.Set(testSocket, new DimensionsComponent(12, 12));
            World.Set(testSocket, new SocketComponent());
            World.Set(testSocket, new DrawLayerComponent(DrawLayer.UserInterface));

            _portraitsByPlayer.Clear();
            foreach (var playerEntity in PlayerFilter.Entities)
            {
                var playerNumber = Get<PlayerNumberComponent>(playerEntity).PlayerNumber;
                var characterNumber = Get<SelectedCharacterComponent>(playerEntity).Character;
                var portraitSprite = characterNumber.ToPortraitSprite();

                var portrait = _menuUtils.CreateUiEntity(
                    PORTRAIT_X,
                    (int) playerNumber * PORTRAIT_HEIGHT + 1 + (int) playerNumber
                );
                Set(portrait, new TextureIndexComponent(portraitSprite));

                _portraitsByPlayer.Add(playerNumber, portrait);
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
        var currentPlayerNumber = Get<PlayerNumberComponent>(GetSingletonEntity<CurrentPlayerFlag>()).PlayerNumber;
        for (var i = 0; i < _numberOfPlayers; i++)
        {
            var portrait = _portraitsByPlayer[currentPlayerNumber];
            var speed = i == _numberOfPlayers - 1 ? 200 : 80;
            Set(portrait, new MovingToScreenPositionComponent(PORTRAIT_X, i * PORTRAIT_HEIGHT + 1 + i, speed));
            currentPlayerNumber = Utils.Utils.GetNextPlayer(currentPlayerNumber, _numberOfPlayers);
        }
    }
}