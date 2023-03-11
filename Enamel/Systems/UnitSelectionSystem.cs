using System;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;

namespace Enamel.Systems;

public class UnitSelectionSystem : MoonTools.ECS.System
{
    private Filter SelectableCoordFilter { get; }
    private Filter DisabledControlledByPlayerFilter { get; }
    private Filter SelectedFilter { get; }

    public UnitSelectionSystem(World world) : base(world)
    {
        SelectableCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Exclude<MovementPreviewFlag>()
            .Exclude<SpellPreviewFlag>()
            .Exclude<DisabledFlag>()
            .Build();
        DisabledControlledByPlayerFilter = FilterBuilder
            .Include<ControlledByPlayerComponent>()
            .Include<DisabledFlag>()
            .Build();
        SelectedFilter = FilterBuilder.Include<SelectedFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        // Can return after handling each message because shouldn't receive more than one of these each frame...
        if (SomeMessage<PrepSpellMessage>())
        {
            foreach (var entity in SelectableCoordFilter.Entities)
            {
                Set(entity, new DisabledFlag());
            }

            return;
        }
        if (SomeMessage<SpellWasCastMessage>())
        {
            var message = ReadMessage<SpellWasCastMessage>();
            var casterPlayerId = Get<ControlledByPlayerComponent>(message.Caster).PlayerNumber;
            foreach (var entity in DisabledControlledByPlayerFilter.Entities)
            {
                // Remove disabled from all units on the caster's team, now that the spell has been cast
                var entityPlayerId = Get<ControlledByPlayerComponent>(entity).PlayerNumber;
                if (casterPlayerId == entityPlayerId)
                {
                    Remove<DisabledFlag>(entity);
                }
            }

            return;
        }
        foreach (var entity in SelectableCoordFilter.Entities)
        {
            if (!SomeMessageWithEntity<SelectMessage>(entity)) continue;

            foreach (var selectedEntity in SelectedFilter.Entities){
                Remove<SelectedFlag>(selectedEntity);
            }

            Set(entity, new SelectedFlag());
            Send(entity, new PlayerUnitSelectedMessage());
        }
    }
}