﻿using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class MenuSystem : MoonTools.ECS.System
{
    private readonly List<Entity> _uiEntities = [];
    private Entity _sheetRow;
    private Entity _addPlayerButton;
    private Entity _deletePlayerButton;

    private Filter PlayerFilter { get; }

    public MenuSystem(World world) : base(world)
    {
        PlayerFilter = FilterBuilder.Include<PlayerNumberComponent>().Build();
    }
    
    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<GoToMainMenuMessage>())
        {
            DestroyExistingUiEntities();
            DestroyAllPlayers();

            var mainMenu = CreateUiEntity(0, 0);
            Set(mainMenu, new TextureIndexComponent(Sprite.TitleScreen));

            var startButton = CreateUiEntity(110, 60, 50, 15);
            Set(startButton, new TextComponent(TextStorage.GetId("Start"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            Set(startButton, new OnClickComponent(ClickEvent.GoToCharacterSelect));

            var optionsButton = CreateUiEntity(110, 80, 50, 15);
            Set(optionsButton, new TextComponent(TextStorage.GetId("Options"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            Set(optionsButton, new OnClickComponent(ClickEvent.OpenOptions));
            
            var quitButton = CreateUiEntity(110, 100, 50, 15);
            Set(quitButton, new TextComponent(TextStorage.GetId("Quit"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            Set(quitButton, new OnClickComponent(ClickEvent.ExitGame));
        }

        if (SomeMessage<GoToCharacterSelectMessage>())
        {
            DestroyExistingUiEntities();

            var mainMenu = CreateUiEntity(0, 0);
            Set(mainMenu, new TextureIndexComponent(Sprite.TitleScreen));
            
            _sheetRow = CreateUiEntity(0, 40);
            Set(_sheetRow, new CenterChildrenComponent(2));

            CreateAddPlayerButton();
            CreateDeletePlayerButton();
            
            // Start with 1 player
            AddPlayer();

            Set(_deletePlayerButton, new DisabledFlag());

            var backButton = CreateUiEntity(80, 110, 50, 15);
            Set(backButton, new TextComponent(TextStorage.GetId("Back"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            Set(backButton, new OnClickComponent(ClickEvent.GoToMainMenu));

            var deployButton = CreateUiEntity(150, 110, 50, 15);
            Set(deployButton, new TextComponent(TextStorage.GetId("Play"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke));
            Set(deployButton, new OnClickComponent(ClickEvent.DeployWizards));
        }

        if (SomeMessage<DeployWizardsMessage>())
        {
            DestroyExistingUiEntities();
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

    private void DestroyAllPlayers()
    {
        foreach (var entity in PlayerFilter.Entities)
        {
            Destroy(entity);
        }
    }

    private void CreateAddPlayerButton()
    {
        _addPlayerButton = CreateUiEntity(80, 40, 40, 30);
        Set(_addPlayerButton, new TextureIndexComponent(Sprite.AddPlayer));
        Set(_addPlayerButton, new OnClickComponent(ClickEvent.AddPlayer));
        Set(_addPlayerButton, new OrderComponent(10)); // This button should always come last in the row, and there shouldn't be more than 10 items
        Relate(_sheetRow, _addPlayerButton, new IsParentRelation());
    }

    private void CreateDeletePlayerButton()
    {
        _deletePlayerButton = CreateUiEntity(80, 70, 40, 30);
        Set(_deletePlayerButton, new TextureIndexComponent(Sprite.DeletePlayer));
        Set(_deletePlayerButton, new OnClickComponent(ClickEvent.DeletePlayer));
        Set(_deletePlayerButton, new OrderComponent(10)); // This button should always come last in the row, and there shouldn't be more than 10 items
        Relate(_sheetRow, _deletePlayerButton, new IsParentRelation());
    }

    private void AddPlayer()
    {
        var existingPlayerCount = PlayerFilter.Count;
        var player = World.CreateEntity();
        var playerNumber = (PlayerNumber)existingPlayerCount;
        Set(player, new PlayerNumberComponent(playerNumber));
        
        var characterSheet = CreateUiEntity(80, 40, 40, 60);
        Set(characterSheet, new TextureIndexComponent(Sprite.CharacterSheet));
        Set(characterSheet, new OrderComponent(existingPlayerCount));
        Relate(_sheetRow, characterSheet, new IsParentRelation());
        Relate(player, characterSheet, new PlayerSheetRelation());

        // The initial position for CreateUiEntity doesn't matter since it'll be set by the RelativePositionSystem anyway
        var leftButton = CreateUiEntity(0, 0, 13, 13);
        Set(leftButton, new RelativePositionComponent(3, 44));
        Set(leftButton, new TextureIndexComponent(Sprite.LeftCharButton));
        Set(leftButton, new AnimationSetComponent(AnimationSet.CharButton));
        Set(leftButton, new ToggleFrameOnMouseDownFlag());
        Relate(characterSheet, leftButton, new IsParentRelation());

        if (existingPlayerCount >= 5)
        {
            Set(_addPlayerButton, new DisabledFlag());
        }
        Remove<DisabledFlag>(_deletePlayerButton);
    }

    private void DeletePlayer(){
        var existingPlayerCount = PlayerFilter.Count;
        
        if(existingPlayerCount <= 2){
            Set(_deletePlayerButton, new DisabledFlag());
        }

        var highestNumberedPlayer = GetHighestNumberedPlayer();
        var sheet = OutRelationSingleton<PlayerSheetRelation>(highestNumberedPlayer);
        
        Destroy(highestNumberedPlayer);
        RecursivelyDestroy(sheet);
        
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

    private void RecursivelyDestroy(Entity entity)
    {
        var children = OutRelations<IsParentRelation>(entity);
        foreach(var child in children){
            RecursivelyDestroy(child);
        }
        Destroy(entity);
    }

    private void DestroyExistingUiEntities(){
        foreach(var entity in _uiEntities){
            Destroy(entity);
        }
        _uiEntities.Clear();
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

        _uiEntities.Add(entity);

        return entity;
    }
}