using System;
using Enamel.Components;
using Enamel.Components.Messages;
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
        foreach (var entity in ControlledByPlayerFilter.Entities)
        {
            if (!SomeMessageWithEntity<StartTurnMessage>(entity)) continue;

            var movesThisTurn = Get<MovesPerTurnComponent>(entity).Amount;
            Set(entity, new RemainingMovesComponent(movesThisTurn));
        }
    }
}