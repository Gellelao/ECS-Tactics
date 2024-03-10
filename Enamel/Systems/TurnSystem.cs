using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.TempComponents;
using MoonTools.ECS;
using static Enamel.Utils.Utils;

namespace Enamel.Systems;

public class TurnSystem : MoonTools.ECS.System
{
    private Filter PlayerFilter { get; }

    public TurnSystem(World world) : base(world)
    {
        PlayerFilter = FilterBuilder.Include<PlayerIdComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<EndTurnMessage>()) return;

        var numberOfPlayers = PlayerFilter.Count;
        var currentPlayer = GetSingletonEntity<CurrentPlayerFlag>();
        Remove<CurrentPlayerFlag>(currentPlayer);
        
        // Get the HandSystem to remove any of the current players orbs that were in play
        Send(new CleanupOrbsInPlayMessage(Get<PlayerIdComponent>(currentPlayer).PlayerId));

        var nextPlayerId = GetNextPlayer(Get<PlayerIdComponent>(currentPlayer).PlayerId, numberOfPlayers);

        foreach (var playerEntity in PlayerFilter.Entities)
        {
            var playerId = Get<PlayerIdComponent>(playerEntity).PlayerId;
            var playersCharacters = OutRelations<ControlsRelation>(playerEntity);
            if (playerId == nextPlayerId)
            {
                Set(playerEntity, new CurrentPlayerFlag());
                
                // Tell the HandSystem to draw the new player's hand
                Send(new DrawHandMessage(playerId));

                // Each unit controlled by the next player becomes selectable
                foreach (var character in playersCharacters)
                {
                    Remove<DisabledFlag>(character);
                }

                // If they just have 1 character then auto-select it
                if (OutRelationCount<ControlsRelation>(playerEntity) == 1)
                {
                    var character = OutRelationSingleton<ControlsRelation>(playerEntity);
                    Set(character, new SelectedFlag());
                    Set(character, new DisplayTomesComponent());
                }
            }
            else
            {
                // Can't select units belonging to other players
                foreach (var character in playersCharacters)
                {
                    Set(character, new DisabledFlag());
                    Remove<SelectedFlag>(character);
                }
            }
        }
    }
}