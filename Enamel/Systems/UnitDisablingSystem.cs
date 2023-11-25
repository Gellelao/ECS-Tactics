using System;
using Enamel.Components;
using Enamel.Components.Messages;
using MoonTools.ECS;

namespace Enamel.Systems;

/*
 * NOTE : Currently some unit disabling also goes on in the TurnSystem, it just fits well there too but would be nice to consolidate it all to here at some point
 */
public class UnitDisablingSystem : MoonTools.ECS.System
{
    private Filter ControlledByPlayerFilter { get; }
    private Filter DisabledControlledByPlayerFilter { get; }
    public UnitDisablingSystem(World world) : base(world)
    {
        ControlledByPlayerFilter = FilterBuilder
            .Include<ControlledByPlayerComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        DisabledControlledByPlayerFilter = FilterBuilder
            .Include<ControlledByPlayerComponent>()
            .Include<DisabledFlag>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<PrepSpellMessage>())
        {
            foreach (var entity in ControlledByPlayerFilter.Entities)
            {
                Set(entity, new DisabledFlag());
            }
        }
        if (SomeMessage<SpellWasCastMessage>() || SomeMessage<CancelMessage>())
        {
            var casterEntity = GetSingletonEntity<SelectedFlag>();
            var casterPlayerId = Get<ControlledByPlayerComponent>(casterEntity).PlayerNumber;
            foreach (var entity in DisabledControlledByPlayerFilter.Entities)
            {
                // Remove disabled from all units on the caster's team, now that the spell has been cast
                var entityPlayerId = Get<ControlledByPlayerComponent>(entity).PlayerNumber;
                if (casterPlayerId == entityPlayerId)
                {
                    Remove<DisabledFlag>(entity);
                }
            }
        }
    }
}