using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.TempComponents;
using Enamel.Enums;
using Enamel.Spawners;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DamageSystem : MoonTools.ECS.System
{
    private readonly ParticleSpawner _particleSpawner;
    private Filter DamageableFilter { get; }

    public DamageSystem(World world, ParticleSpawner particleSpawner) : base(world)
    {
        _particleSpawner = particleSpawner;
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
                if (Has<ScreenPositionComponent>(entity))
                {
                    var screenPos = Get<ScreenPositionComponent>(entity).ToVector;
                    _particleSpawner.SpawnSmokePuff(screenPos.X, screenPos.Y, ScreenDirection.Up, 20);
                }
                Destroy(entity);
            }
            else
            {
                Set(entity, new HealthComponent(newHealth));
                Set(entity, new TempAnimationComponent(AnimationType.Hurt, AnimationType.Idle));
                Remove<TakingDamageComponent>(entity);
            }
        }
    }
}