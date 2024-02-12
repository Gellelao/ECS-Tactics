using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using MoonTools.ECS;

namespace Enamel.Systems;

/*
 * NOTE : Currently some unit disabling also goes on in the TurnSystem, it just fits well there too but would be nice to consolidate it all to here at some point
 */
public class DisablingSystem : MoonTools.ECS.System
{
    public Filter OnClickFilter { get; }

    public DisablingSystem(World world) : base(world)
    {
        OnClickFilter = FilterBuilder.Include<OnClickComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<PrepSpellMessage>())
        {
            // You shouldn't be able to select units when casting a spell
            foreach (var (_, character) in Relations<ControlsRelation>())
            {
                Set(character, new DisabledFlag());
            }

            // Also can't click buttons while a spell is ready to cast
            foreach (var button in OnClickFilter.Entities)
            {
                Set(button, new DisabledFlag());
            }
        }

        if (SomeMessage<SpellWasCastMessage>() || SomeMessage<CancelMessage>())
        {
            // Assume the spell was cast by the current player
            var currentPlayer = GetSingletonEntity<CurrentPlayerFlag>();
            if (!Has<SelectedCharacterComponent>(currentPlayer))
            {
                // If the player has a "selected character" we know they are deploying, so leave their characters disabled
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

            // Do buttons too
            // Has the potential to be bad if there are buttons that should stay disabled for other reasons
            foreach (var button in OnClickFilter.Entities)
            {
                Remove<DisabledFlag>(button);
            }
        }
    }
}