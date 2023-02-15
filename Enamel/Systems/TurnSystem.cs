using System;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class TurnSystem : MoonTools.ECS.System
{
    private Filter ControlledByPlayerFilter { get; }
    private int _currentPlayerTurnIndex;

    public TurnSystem(World world) : base(world)
    {
        ControlledByPlayerFilter = FilterBuilder.Include<ControlledByPlayerComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<EndTurnMessage>()) return;

        // At this point, we know we can proceed with ending the turn, and no longer need to the entity, which is
        // the button that was clicked
        var numberOfPlayers = ControlledByPlayerFilter.Entities.Count();
        IncrementTurnIndex(numberOfPlayers);
        var nextPlayer = GetTurnOrder(numberOfPlayers)[_currentPlayerTurnIndex];

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

    private void IncrementTurnIndex(int numberOfPlayers)
    {
        var turnOrder = GetTurnOrder(numberOfPlayers);
        _currentPlayerTurnIndex++;
        if (_currentPlayerTurnIndex >= turnOrder.Length) _currentPlayerTurnIndex = 0;
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