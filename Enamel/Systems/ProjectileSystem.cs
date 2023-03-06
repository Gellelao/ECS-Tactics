using System;
using System.Collections.Generic;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class ProjectileSystem : MoonTools.ECS.System
{
    private readonly World _world;
    private Filter MovingInDirectionFilter { get; }
    private Filter GridCoordFilter { get; }

    public ProjectileSystem(World world) : base(world)
    {
        _world = world;
        MovingInDirectionFilter = FilterBuilder.Include<MovingInDirectionComponent>().Build();
        GridCoordFilter = FilterBuilder.Include<GridCoordComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var movingEntity in MovingInDirectionFilter.Entities)
        {
            var direction = Get<MovingInDirectionComponent>(movingEntity).Direction;
            var damage = Get<ProjectileDamageComponent>(movingEntity).Damage;
            if (!Has<GridCoordComponent>(movingEntity)) continue;

            var (gridX, gridY) = Get<GridCoordComponent>(movingEntity);
            switch (direction)
            {
                case Direction.North:
                    gridY -= 1;
                    break;
                case Direction.East:
                    gridX += 1;
                    break;
                case Direction.South:
                    gridY += 1;
                    break;
                case Direction.West:
                    gridX -= 1;
                    break;
                case Direction.None:
                default:
                    // Should not have Direction.None at this point, so throw
                    throw new ArgumentOutOfRangeException();
            }

            var entitiesAtDestination = GetEntitiesAtCoords(gridX, gridY);
            if (entitiesAtDestination.Any())
            {
                var screenPos = Get<PositionComponent>(entitiesAtDestination.First());
                _world.Set(movingEntity, new MovingToPositionComponent(screenPos.X, screenPos.Y, gridX, gridY));
                Remove<GridCoordComponent>(movingEntity);

                var impassableEntities = entitiesAtDestination.Where(e => Has<ImpassableFlag>(e)).ToList();
                if (impassableEntities.Any())
                {
                    Send(impassableEntities.First(), new DamageMessage(damage));
                    Destroy(movingEntity);
                }
            }
            else
            {
                // Must be at edge of map because there should at least be a GroundTile at every valid coord
                Console.WriteLine($"Destroying projectile because it reached {gridX},{gridY}");
                Destroy(movingEntity);
            }
        }
    }

    private List<Entity> GetEntitiesAtCoords(int x, int y)
    {
        var entities = new List<Entity>();
        foreach (var entity in GridCoordFilter.Entities)
        {
            var (gridCoordEntityX, gridCoordEntityY) = Get<GridCoordComponent>(entity);
            if (gridCoordEntityX == x && gridCoordEntityY == y)
            {
                  entities.Add(entity);
            }
        }

        return entities;
    }
}