using System;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class TurnSystem : MoonTools.ECS.System
{
    private Filter ControlledByPlayerFilter { get; }

    public TurnSystem(World world) : base(world)
    {
        ControlledByPlayerFilter = FilterBuilder.Include<ControlledByPlayerComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<EndTurnMessage>()) return;

        // At this point there must be an end turn message, so proceed with ending the turn
        var numberOfPlayers = GetSingleton<PlayerCountComponent>().NumberOfPlayers;
        var turnIndex = IncrementTurnIndex(numberOfPlayers);
        var nextPlayer = GetTurnOrder(numberOfPlayers)[turnIndex];

        foreach (var entity in ControlledByPlayerFilter.Entities)
        {
            var controllingPlayer = Get<ControlledByPlayerComponent>(entity);
            if (controllingPlayer.PlayerNumber == nextPlayer)
            {
                // The next player becomes selectable and has their moves per turn reset
                var movesThisTurn = Get<MovesPerTurnComponent>(entity).Amount;
                Set(entity, new RemainingMovesComponent(movesThisTurn));
                Remove<DisabledFlag>(entity);
            }
            else
            {
                // Can't select units belonging to other players
                Set(entity, new DisabledFlag());
                Remove<SelectedFlag>(entity);
            }
        }
    }

    private int IncrementTurnIndex(int numberOfPlayers)
    {
        var turnTracker = GetSingletonEntity<TurnIndexComponent>();
        var turnIndex = Get<TurnIndexComponent>(turnTracker).Index;
        turnIndex++;

        var turnOrder = GetTurnOrder(numberOfPlayers);
        if (turnIndex >= turnOrder.Length) turnIndex = 0;

        Set(turnTracker, new TurnIndexComponent(turnIndex));
        Set(turnTracker, new TextComponent(
            TextStorage.GetId($"Current player: {turnOrder[turnIndex]}"),
            Constants.CURRENT_TURN_TEXT_SIZE,
            Constants.CURRENT_TURN_TEXT_COLOUR)
        );
        return turnIndex;
    }

    private static PlayerNumber[] GetTurnOrder(int numberOfPlayers)
    {
        return numberOfPlayers switch
        {
            2 => Constants.TwoPlayerTurnOrder,
            3 => Constants.ThreePlayerTurnOrder,
            4 => Constants.FourPlayerTurnOrder,
            _ => throw new ArgumentOutOfRangeException(nameof(numberOfPlayers))
        };
    }
}