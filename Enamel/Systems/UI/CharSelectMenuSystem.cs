using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Extensions;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems.UI;

public class CharSelectMenuSystem : MoonTools.ECS.System
{
    private readonly MenuUtils _menuUtils;
    private readonly int _numberOfCharacters;
    private Entity _sheetRow;
    private Entity _addPlayerButton;
    private Entity _deletePlayerButton;

    private Filter PlayerFilter { get; }

    public CharSelectMenuSystem(World world, MenuUtils menuUtils) : base(world)
    {
        _menuUtils = menuUtils;
        _numberOfCharacters = Enum.GetNames(typeof(CharacterId)).Length;
        PlayerFilter = FilterBuilder.Include<PlayerIdComponent>().Build();
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
            Set(
                backButton,
                new TextComponent(TextStorage.GetId("Back"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke)
            );
            Set(backButton, new OnClickComponent(ClickEvent.GoToMainMenu));

            var deployButton = _menuUtils.CreateUiEntity(150, 110, 50, 15);
            Set(
                deployButton,
                new TextComponent(TextStorage.GetId("Play"), Font.Absolute, Microsoft.Xna.Framework.Color.WhiteSmoke)
            );
            Set(deployButton, new OnClickComponent(ClickEvent.DeployWizards));
        }

        if (SomeMessage<DeployWizardsMessage>())
        {
            _menuUtils.DestroyExistingUiEntities();
            var startingPlayer = PlayerFilter.RandomEntity;
            Set(startingPlayer, new CurrentPlayerFlag());
        }

        if (SomeMessage<AddPlayerMessage>())
        {
            AddPlayer();
        }

        if (SomeMessage<DeletePlayerMessage>())
        {
            DeletePlayer();
        }

        if (SomeMessage<PreviousCharacterMessage>())
        {
            var player = ReadMessage<PreviousCharacterMessage>().PlayerId;
            CycleCharacterForPlayer(player, -1);
        }

        if (SomeMessage<NextCharacterMessage>())
        {
            var player = ReadMessage<NextCharacterMessage>().PlayerId;
            CycleCharacterForPlayer(player, 1);
        }
    }

    private void CreateAddPlayerButton()
    {
        _addPlayerButton = _menuUtils.CreateUiEntity(80, 40, 40, 30);
        Set(_addPlayerButton, new TextureIndexComponent(Sprite.AddPlayer));
        Set(_addPlayerButton, new OnClickComponent(ClickEvent.AddPlayer));
        // This button should always come last in the row, and there shouldn't be more than 10 items, so an order of 10 is fine
        Set(_addPlayerButton, new OrderComponent(10));
        Relate(_sheetRow, _addPlayerButton, new IsParentRelation());
    }

    private void CreateDeletePlayerButton()
    {
        _deletePlayerButton = _menuUtils.CreateUiEntity(80, 70, 40, 30);
        Set(_deletePlayerButton, new TextureIndexComponent(Sprite.DeletePlayer));
        Set(_deletePlayerButton, new OnClickComponent(ClickEvent.DeletePlayer));
        // This button should always come last in the row, and there shouldn't be more than 10 items, so an order of 10 is fine
        Set(_deletePlayerButton, new OrderComponent(10));
        Relate(_sheetRow, _deletePlayerButton, new IsParentRelation());
    }

    private void AddPlayer()
    {
        var existingPlayerCount = PlayerFilter.Count;
        var playerEntity = World.CreateEntity();
        var playerId = (PlayerId) existingPlayerCount;
        Set(playerEntity, new PlayerIdComponent(playerId));
        Set(playerEntity, new SelectedCharacterComponent(Constants.DEFAULT_CHARACTER_ID));
        Send(new SetStartingOrbsForPlayerMessage(Get<PlayerIdComponent>(playerEntity).PlayerId, Constants.DEFAULT_CHARACTER_ID));
        Set(playerEntity, new HandSizeComponent(Constants.DEFAULT_HAND_SIZE));

        CreateCharacterSheetForPlayer(playerEntity);

        if (existingPlayerCount >= 5)
        {
            Set(_addPlayerButton, new DisabledFlag());
        }

        Remove<DisabledFlag>(_deletePlayerButton);
    }

    private void CycleCharacterForPlayer(PlayerId playerId, int increment)
    {
        foreach (var playerEntity in PlayerFilter.Entities)
        {
            if (Get<PlayerIdComponent>(playerEntity).PlayerId != playerId) continue;
            
            DeleteCharacterSheetForPlayer(playerEntity);
            
            var selectedCharacter = (int) Get<SelectedCharacterComponent>(playerEntity).CharacterId;
            var newCharacter = (CharacterId) ((selectedCharacter + increment + _numberOfCharacters) % _numberOfCharacters);
            
            Set(playerEntity, new SelectedCharacterComponent(newCharacter));
            Send(new SetStartingOrbsForPlayerMessage(Get<PlayerIdComponent>(playerEntity).PlayerId, newCharacter));
            CreateCharacterSheetForPlayer(playerEntity);
        }
    }

    private void DeleteCharacterSheetForPlayer(Entity playerEntity)
    {
        var children = OutRelations<IsParentRelation>(playerEntity);
        foreach (var child in children)
        {
            _menuUtils.RecursivelyDestroy(child);
        }
    }

    private void CreateCharacterSheetForPlayer(Entity playerEntity)
    {
        var characterSheet = _menuUtils.CreateUiEntity(80, 40, 40, 60);
        Set(characterSheet, new TextureIndexComponent(Sprite.CharacterSheet));

        var playerId = Get<PlayerIdComponent>(playerEntity).PlayerId;

        Set(characterSheet, new OrderComponent((int) playerId));
        Relate(_sheetRow, characterSheet, new IsParentRelation());
        Relate(playerEntity, characterSheet, new IsParentRelation());

        var leftButton = _menuUtils.CreateRelativeUiEntity(characterSheet, 3, 44, 13, 13);
        Set(leftButton, new TextureIndexComponent(Sprite.LeftCharButton));
        Set(leftButton, new AnimationSetComponent(AnimationSet.CharButton));
        Set(leftButton, new ToggleFrameOnMouseHoverComponent(1));
        Set(leftButton, new ToggleFrameOnMouseDownComponent(2));
        Set(leftButton, new OnClickComponent(ClickEvent.PreviousCharacter, (int) playerId));

        var rightButton = _menuUtils.CreateRelativeUiEntity(characterSheet, 24, 44, 13, 13);
        Set(rightButton, new TextureIndexComponent(Sprite.RightCharButton));
        Set(rightButton, new AnimationSetComponent(AnimationSet.CharButton));
        Set(rightButton, new ToggleFrameOnMouseHoverComponent(1));
        Set(rightButton, new ToggleFrameOnMouseDownComponent(2));
        Set(rightButton, new OnClickComponent(ClickEvent.NextCharacter, (int) playerId));

        var character = Get<SelectedCharacterComponent>(playerEntity).CharacterId;

        var characterPreview = _menuUtils.CreateRelativeUiEntity(characterSheet, 10, 0, 13, 13);
        Set(characterPreview, new TextureIndexComponent(character.ToCharacterSprite()));
        Set(characterPreview, new AnimationSetComponent(AnimationSet.Wizard));
        Set(characterPreview, new AnimationStatusComponent(AnimationType.Idle, AnimationType.Idle, double.MaxValue));
    }

    private void DeletePlayer()
    {
        var existingPlayerCount = PlayerFilter.Count;

        if (existingPlayerCount <= 2)
        {
            Set(_deletePlayerButton, new DisabledFlag());
        }

        var highestNumberedPlayer = GetHighestNumberedPlayer();

        _menuUtils.RecursivelyDestroy(highestNumberedPlayer);

        Remove<DisabledFlag>(_addPlayerButton);
    }

    private Entity GetHighestNumberedPlayer()
    {
        var highest = 0;
        Entity? highestPlayer = null;

        foreach (var player in PlayerFilter.Entities)
        {
            var playerId = Get<PlayerIdComponent>(player).PlayerId;
            if ((int) playerId > highest)
            {
                highest = (int) playerId;
                highestPlayer = player;
            }
        }

        if (highestPlayer == null)
        {
            throw new InvalidOperationException("There should be at least one player");
        }

        return (Entity) highestPlayer;
    }
}