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
public class UnitDisablingSystem(World world) : MoonTools.ECS.System(world)
{
    public override void Update(TimeSpan delta)
    {
        // You shouldn't be able to select units when casting a spell
        if (SomeMessage<PrepSpellMessage>())
        {
            foreach (var (_,  character) in Relations<ControlsRelation>())
            {
                Set(character, new DisabledFlag());
            }
        }
        if (SomeMessage<SpellWasCastMessage>() || SomeMessage<CancelMessage>())
        {
            // Assume the spell was cast by the current player
            var currentPlayer = GetSingletonEntity<CurrentPlayerFlag>();
            var currentPlayerNumber = Get<PlayerNumberComponent>(currentPlayer).PlayerNumber;
            foreach (var (player, character) in Relations<ControlsRelation>())
            {
                // Remove disabled from all units on the caster's team, now that the spell has been cast
                var controllerNumber = Get<PlayerNumberComponent>(player).PlayerNumber;
                if (controllerNumber == currentPlayerNumber)
                {
                    Remove<DisabledFlag>(character);
                }
            }
        }
    }
}