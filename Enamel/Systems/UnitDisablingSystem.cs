using System;
using System.Collections.Generic;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Enums;
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
            .Include<ControlsRelation>()
            .Exclude<DisabledFlag>()
            .Build();
        DisabledControlledByPlayerFilter = FilterBuilder
            .Include<ControlsRelation>()
            .Include<DisabledFlag>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        // You shouldn't be able to select units when casting a spell
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
            var playerControllingCaster = GetPlayerNumbersControllingEntity(casterEntity);
            foreach (var entity in DisabledControlledByPlayerFilter.Entities)
            {
                // Remove disabled from all units on the caster's team, now that the spell has been cast
                var playersControllingEntity = GetPlayerNumbersControllingEntity(entity);
                if (playerControllingCaster.Intersect(playersControllingEntity).Any())
                {
                    Remove<DisabledFlag>(entity);
                }
            }
        }
    }

    private List<Player> GetPlayerNumbersControllingEntity(Entity entity)
    {
        var entitiesControllingEntity = InRelations<ControlsRelation>(entity);
        var playersControllingEntity = new List<Player>();
        foreach (var controller in entitiesControllingEntity)
        {
            playersControllingEntity.Add(Get<PlayerNumberComponent>(controller).PlayerNumber);
        }

        return playersControllingEntity;
    }
}