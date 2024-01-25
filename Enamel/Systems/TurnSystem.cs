using System;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class TurnSystem : MoonTools.ECS.System
{
    private Filter PlayerFilter { get; }

    public TurnSystem(World world) : base(world)
    {
        PlayerFilter = FilterBuilder.Include<PlayerNumberComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<EndTurnMessage>()) return;

        var numberOfPlayers = PlayerFilter.Count;
        var turnIndex = IncrementTurnIndex(numberOfPlayers);
        var nextPlayer = GetTurnOrder(numberOfPlayers)[turnIndex];

        foreach (var player in PlayerFilter.Entities)
        {
            var playerNumber = Get<PlayerNumberComponent>(player).PlayerNumber;
            var playersCharacters = OutRelations<ControlsRelation>(player);
            if (playerNumber == nextPlayer)
            {
                // Each unit controlled by the next player becomes selectable
                foreach (var character in playersCharacters)
                {
                    Remove<DisabledFlag>(character);
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
            Font.Absolute,
            Constants.CurrentTurnTextColour)
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