using System;
using Enamel.Components;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DestroyAfterDurationSystem : MoonTools.ECS.System
{
    private Filter DestroyableFilter { get; }

    public DestroyAfterDurationSystem(World world) : base(world)
    {
        DestroyableFilter = FilterBuilder
            .Include<DestroyAfterMillisComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in DestroyableFilter.Entities)
        {
            var destroyMillisComponent = Get<DestroyAfterMillisComponent>(entity);
            var increasedMillis = destroyMillisComponent.CurrentMillis + delta.TotalMilliseconds;
            if (increasedMillis > destroyMillisComponent.TotalMillis)
            {
                Destroy(entity);
            }
            else
            {
                Set(entity, destroyMillisComponent with {CurrentMillis = increasedMillis});
            }
        }
    }
}