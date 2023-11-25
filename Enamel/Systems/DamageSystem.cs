using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.TempComponents;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DamageSystem : MoonTools.ECS.System
{
    private Filter DamageableFilter { get; }

    public DamageSystem(World world) : base(world)
    {
        DamageableFilter = FilterBuilder
            .Include<HealthComponent>()
            .Include<TakingDamageComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in DamageableFilter.Entities)
        {
            // Could return early here if entity has "InvulnerableComponent" but don't have a use case yet
            var amountOfDamage = Get<TakingDamageComponent>(entity).AmountOfDamage;
            var currentHealth = Get<HealthComponent>(entity).RemainingHealth;
            var newHealth = currentHealth;
            
            newHealth -= amountOfDamage;

            if (newHealth <= 0)
            {
                // Send some DeathMessage or something idk
                Destroy(entity);
            }
            else
            {
                Set(entity, new HealthComponent(newHealth));
                Set(entity, new AnimationTypeComponent(AnimationType.Hurt));
                // Some component to stop the hurt  animation after a short while
                Remove<TakingDamageComponent>(entity);
            }
        }
    }
}