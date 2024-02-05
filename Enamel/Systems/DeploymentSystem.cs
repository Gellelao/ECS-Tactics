using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Spawners;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DeploymentSystem : MoonTools.ECS.System
{
    private readonly CharacterSpawner _characterSpawner;
    private Entity _deployingPlayer;
    private Filter PlayersWaitingToDeploy { get; }
    
    public DeploymentSystem(World world, CharacterSpawner characterSpawner) : base(world)
    {
        _characterSpawner = characterSpawner;
        PlayersWaitingToDeploy = FilterBuilder.Include<SelectedCharacterComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (PlayersWaitingToDeploy.Empty) return;

        if(SomeMessage<EndTurnMessage>())
        {
            _deployingPlayer = GetSingletonEntity<CurrentPlayerFlag>();
            Remove<SelectedCharacterComponent>(_deployingPlayer);
            return;
        }

        // Kind jank but we need to display the previews for deploying the wizard in 3 situations:
        // 1. When deployment first starts
        // 2. After a character is placed, in case they want to replace the character somewhere else
        // 3. After the end turn button is clicked if there are more wizards to place
        if (Some<CurrentPlayerFlag>() && !Some<SpellToCastOnSelectComponent>())
        {
            Send(new PrepSpellMessage(SpellId.DeployWizard, 0, 0));
        }
        
        if (!SomeMessage<GridCoordSelectedMessage>()) return;
        
        // If the current player clicks, delete any existing characters and place their selected char
        _deployingPlayer = GetSingletonEntity<CurrentPlayerFlag>();
        DestroyCharactersOfPlayer(_deployingPlayer);
        var characterToDeploy = Get<SelectedCharacterComponent>(_deployingPlayer).Character;

        var coords = ReadMessage<GridCoordSelectedMessage>();
        Entity character = _characterSpawner.SpawnCharacter(characterToDeploy, coords.X, coords.Y);
        Relate(_deployingPlayer, character, new ControlsRelation());
    }

    private void DestroyCharactersOfPlayer(Entity deployingPlayer)
    {
        foreach (var character in OutRelations<ControlsRelation>(deployingPlayer))
        {
            Destroy(character);
        }
    }
}