﻿using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems.UI;

public class CharSelectMenuSystem : MoonTools.ECS.System
{
    private readonly MenuUtils _menuUtils;
    private Entity _sheetRow;
    private Entity _addPlayerButton;
    private Entity _deletePlayerButton;

    private Filter PlayerFilter { get; }

    public CharSelectMenuSystem(World world, MenuUtils menuUtils) : base(world)
    {
        _menuUtils = menuUtils;
        PlayerFilter = FilterBuilder.Include<PlayerNumberComponent>().Build();
    }
    
    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<GoToCharacterSelectMessage>())
        {
            _menuUtils.DestroyExistingUiEntities();

            var mainMenu = _menuUtils.CreateUiEntity(0, 0);
            Set(mainMenu, new TextureIndexComponent(Sprite.TitleScreen));
            
            _sheetRow = _menuUtils.CreateUiEntity(0, 40);
            Set(_sheetRow, new CenterChildrenComponent(2));

            CreateAddPlayerButton();
            CreateDeletePlayerButton();
            
            // Start with 1 player
            AddPlayer();

            Set(_deletePlayerButton, new DisabledFlag());

            var backButton = _menuUtils.CreateUiEntity(80, 110, 50, 15);
            Set(backButton, new TextComponent(TextStorage.GetId("Back"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            Set(backButton, new OnClickComponent(ClickEvent.GoToMainMenu));

            var deployButton = _menuUtils.CreateUiEntity(150, 110, 50, 15);
            Set(deployButton, new TextComponent(TextStorage.GetId("Play"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            Set(deployButton, new OnClickComponent(ClickEvent.DeployWizards));
        }
        
        if (SomeMessage<DeployWizardsMessage>())
        {
            _menuUtils.DestroyExistingUiEntities();
        }

        if (SomeMessage<AddPlayerMessage>())
        {
            AddPlayer();
        }

        if (SomeMessage<DeletePlayerMessage>())
        {
            DeletePlayer();
        }
    }

    private void CreateAddPlayerButton()
    {
        _addPlayerButton = _menuUtils.CreateUiEntity(80, 40, 40, 30);
        Set(_addPlayerButton, new TextureIndexComponent(Sprite.AddPlayer));
        Set(_addPlayerButton, new OnClickComponent(ClickEvent.AddPlayer));
        Set(_addPlayerButton, new OrderComponent(10)); // This button should always come last in the row, and there shouldn't be more than 10 items
        Relate(_sheetRow, _addPlayerButton, new IsParentRelation());
    }

    private void CreateDeletePlayerButton()
    {
        _deletePlayerButton = _menuUtils.CreateUiEntity(80, 70, 40, 30);
        Set(_deletePlayerButton, new TextureIndexComponent(Sprite.DeletePlayer));
        Set(_deletePlayerButton, new OnClickComponent(ClickEvent.DeletePlayer));
        Set(_deletePlayerButton, new OrderComponent(10)); // This button should always come last in the row, and there shouldn't be more than 10 items
        Relate(_sheetRow, _deletePlayerButton, new IsParentRelation());
    }

    private void AddPlayer()
    {
        var existingPlayerCount = PlayerFilter.Count;
        var playerEntity = World.CreateEntity();
        var playerNumber = (Player)existingPlayerCount;
        Set(playerEntity, new PlayerNumberComponent(playerNumber));
        
        var characterSheet = _menuUtils.CreateUiEntity(80, 40, 40, 60);
        Set(characterSheet, new TextureIndexComponent(Sprite.CharacterSheet));
        Set(characterSheet, new OrderComponent(existingPlayerCount));
        Relate(_sheetRow, characterSheet, new IsParentRelation());
        Relate(playerEntity, characterSheet, new PlayerSheetRelation());

        // The initial position for CreateUiEntity doesn't matter since it'll be set by the RelativePositionSystem anyway
        var leftButton = _menuUtils.CreateRelativeUiEntity(3, 44, 13, 13);
        Set(leftButton, new TextureIndexComponent(Sprite.LeftCharButton));
        Set(leftButton, new AnimationSetComponent(AnimationSet.CharButton));
        Set(leftButton, new ToggleFrameOnMouseHoverComponent(1, true));
        Set(leftButton, new ToggleFrameOnMouseDownComponent(2, true));
        Relate(characterSheet, leftButton, new IsParentRelation());
        
        var rightButton = _menuUtils.CreateRelativeUiEntity(24, 44, 13, 13);
        Set(rightButton, new TextureIndexComponent(Sprite.RightCharButton));
        Set(rightButton, new AnimationSetComponent(AnimationSet.CharButton));
        Set(rightButton, new ToggleFrameOnMouseHoverComponent(1, true));
        Set(rightButton, new ToggleFrameOnMouseDownComponent(2, true));
        Relate(characterSheet, rightButton, new IsParentRelation());
        
        var characterPreview = _menuUtils.CreateRelativeUiEntity(10, 0, 13, 13);
        Set(characterPreview, new TextureIndexComponent(Sprite.BlueWizard));
        Set(characterPreview, new AnimationSetComponent(AnimationSet.BlueWiz));
        Set(characterPreview, new FacingDirectionComponent(GridDirection.South));
        Set(characterPreview, new AnimationStatusComponent(AnimationType.Idle, AnimationType.Idle, double.MaxValue));
        Relate(characterSheet, characterPreview, new IsParentRelation());

        if (existingPlayerCount >= 5)
        {
            Set(_addPlayerButton, new DisabledFlag());
        }
        Remove<DisabledFlag>(_deletePlayerButton);
    }

    private void CreateCharacterSheetUI()
    {
        
    }

    private void DeletePlayer(){
        var existingPlayerCount = PlayerFilter.Count;
        
        if(existingPlayerCount <= 2){
            Set(_deletePlayerButton, new DisabledFlag());
        }

        var highestNumberedPlayer = GetHighestNumberedPlayer();
        var sheet = OutRelationSingleton<PlayerSheetRelation>(highestNumberedPlayer);
        
        Destroy(highestNumberedPlayer);
        _menuUtils.RecursivelyDestroy(sheet);
        
        Remove<DisabledFlag>(_addPlayerButton);
    }

    private Entity GetHighestNumberedPlayer()
    {
        var highest = 0;
        Entity? highestPlayer = null;

        foreach(var player in PlayerFilter.Entities)
        {
            var playerNumber = Get<PlayerNumberComponent>(player).PlayerNumber;
            if((int)playerNumber > highest)
            {
                highest = (int)playerNumber;
                highestPlayer = player;
            }
        }

        if(highestPlayer == null)
        {
            throw new InvalidOperationException("There should be at least one player");
        }

        return (Entity)highestPlayer;
    }
}