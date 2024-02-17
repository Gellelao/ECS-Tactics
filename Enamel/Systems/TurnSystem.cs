using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
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

        var nextPlayerId = GetNextPlayer(Get<PlayerIdComponent>(currentPlayer).PlayerId, numberOfPlayers);

        foreach (var playerEntity in PlayerFilter.Entities)
        {
            var playerId = Get<PlayerIdComponent>(playerEntity).PlayerId;
            var playersCharacters = OutRelations<ControlsRelation>(playerEntity);
            if (playerId == nextPlayerId)
            {
                Set(playerEntity, new CurrentPlayerFlag());

                // Each unit controlled by the next player becomes selectable
                foreach (var character in playersCharacters)
                {
                    Remove<DisabledFlag>(character);
                }

                // If they just have 1 character then auto-select it
                if (OutRelationCount<ControlsRelation>(playerEntity) == 1)
                {
                    Set(OutRelationSingleton<ControlsRelation>(playerEntity), new SelectedFlag());
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