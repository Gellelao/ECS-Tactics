using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Enums;
using Enamel.Spawners;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DeploymentSystem : MoonTools.ECS.System
{
    private readonly CharacterSpawner _characterSpawner;
    private Filter PlayersWaitingToDeploy { get; }
    
    public DeploymentSystem(World world, CharacterSpawner characterSpawner) : base(world)
    {
        _characterSpawner = characterSpawner;
        PlayersWaitingToDeploy = FilterBuilder.Include<SelectedCharacterComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<GridCoordSelectedMessage>()) return;
        if (!Some<CurrentlyDeployingFlag>()) return;

        var deployingPlayer = GetSingletonEntity<CurrentlyDeployingFlag>();
        var characterToDeploy = Get<SelectedCharacterComponent>(deployingPlayer).Character;

        var coords = ReadMessage<GridCoordSelectedMessage>();
        Entity character = _characterSpawner.SpawnCharacter(characterToDeploy, coords.X, coords.Y);
        Relate(deployingPlayer, character, new ControlsRelation());
        
        Remove<CurrentlyDeployingFlag>(deployingPlayer);
        Remove<SelectedCharacterComponent>(deployingPlayer);

        if (PlayersWaitingToDeploy.Empty)
        {
            // This kicks off the turn system
            Send(new EndTurnMessage());
        }
        else
        {
            Set(PlayersWaitingToDeploy.RandomEntity, new CurrentlyDeployingFlag());
            Send(new PrepSpellMessage(SpellId.DeployWizard, 0, 0));
        }
    }
}