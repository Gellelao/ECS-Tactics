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
            if (Has<ProjectileMoveRateComponent>(movingEntity))
            {
                switch (Get<ProjectileMoveRateComponent>(movingEntity).Rate)
                {
                    case ProjectileMoveRate.Immediate:
                        MoveOnce(movingEntity);
                        break;
                   case ProjectileMoveRate.PerStep:
                       if (SomeMessage<SpellWasCastMessage>() || SomeMessage<UnitMoveCompletedMessage>())
                       {
                           MoveOnce(movingEntity);
                       }
                       break;
                    case ProjectileMoveRate.PerEvenStep:
                        throw new NotImplementedException(
                            "Haven't added PerEvenStep projectile movement yet because not sure its a good idea");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void MoveOnce(Entity movingEntity)
    {
        if (!Has<GridCoordComponent>(movingEntity)) return;
        var (gridX, gridY) = Get<GridCoordComponent>(movingEntity);

        var entityDestroyed = HandleCollisionAndOutOfBounds(movingEntity, gridX, gridY);
        if (entityDestroyed) return;

        var direction = Get<MovingInDirectionComponent>(movingEntity).Direction;

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

        _world.Set(movingEntity, new MovingToCoordComponent(gridX, gridY));
        Remove<GridCoordComponent>(movingEntity);
    }

    private bool HandleCollisionAndOutOfBounds(Entity movingEntity, int gridX, int gridY)
    {
        var damage = Has<ProjectileDamageComponent>(movingEntity) ? Get<ProjectileDamageComponent>(movingEntity).Damage : 0;
        var entitiesAtLocation = GetEntitiesAtCoords(gridX, gridY);
        // It actually looks quite good with projectiles travelling past the edge of the screen, but need to destroy them eventually, so for now just destroy once out of bounds of the map
        entitiesAtLocation.Remove(movingEntity);
        if (entitiesAtLocation.Any())
        {
            var impassableEntities = entitiesAtLocation.Where(e => Has<ImpassableFlag>(e)).ToList();
            if (impassableEntities.Any())
            {
                Send(impassableEntities.First(), new DamageMessage(damage));
                Destroy(movingEntity);
                return true;
            }
        }
        else
        {
            // Must be at edge of map because there should at least be a GroundTile at every valid coord
            Console.WriteLine($"Destroying projectile because it reached {gridX},{gridY}");
            Destroy(movingEntity);
            return true;
        }

        return false;
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