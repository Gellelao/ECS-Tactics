using System;
using Enamel.Components;
using Enamel.Components.Spells.SpawnedEntities;
using MoonTools.ECS;

namespace Enamel.Systems;

public class ProjectileSystem : MoonTools.ECS.System
{
    private Filter MovingInDirectionFilter { get; }

    public ProjectileSystem(World world) : base(world)
    {
        MovingInDirectionFilter = FilterBuilder.Include<MovingInDirectionComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in MovingInDirectionFilter.Entities)
        {
            var direction = Get<MovingInDirectionComponent>(entity).Direction;
            if (Has<GridCoordComponent>(entity))
            {
                var (x, y) = Get<GridCoordComponent>(entity);

            }
        }
    }
}