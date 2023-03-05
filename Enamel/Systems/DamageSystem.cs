using System;
using Enamel.Components;
using Enamel.Components.Messages;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DamageSystem : MoonTools.ECS.System
{
    private Filter HealthFilter { get; }

    public DamageSystem(World world) : base(world)
    {
        HealthFilter = FilterBuilder.Include<HealthComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entityWithHealth in HealthFilter.Entities)
        {
            // Could return early here if entity has "InvulnerableComponent" but don't have a use case yet
            if (SomeMessageWithEntity<DamageMessage>(entityWithHealth))
            {
                // Could be 2 things damaging a unit at the same time...
                var damageMessages = ReadMessagesWithEntity<DamageMessage>(entityWithHealth);
                var currentHealth = Get<HealthComponent>(entityWithHealth).RemainingHealth;
                var newHealth = currentHealth;

                foreach (var message in damageMessages)
                {
                    newHealth -= message.AmountOfDamage;
                }

                if (newHealth <= 0)
                {
                    // Send some DeathMessage or something idk
                    Destroy(entityWithHealth);
                }
                else
                {
                    Set(entityWithHealth, new HealthComponent(newHealth));
                }
            }
        }
    }
}