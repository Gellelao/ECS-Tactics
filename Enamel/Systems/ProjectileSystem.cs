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

        var shouldMoveEntity = HandleCollisionAndOutOfBounds(movingEntity, direction, gridX, gridY);
        if (!shouldMoveEntity) return;

        _world.Set(movingEntity, new MovingToCoordComponent(gridX, gridY));
        Remove<GridCoordComponent>(movingEntity);
    }

    private bool HandleCollisionAndOutOfBounds(Entity movingEntity, Direction direction, int candidateX, int candidateY)
    {
        var entitiesAtLocation = GetEntitiesAtCoords(candidateX, candidateY);

        if (entitiesAtLocation.Any())
        {
            var impassableEntities = entitiesAtLocation.Where(e => Has<ImpassableFlag>(e)).ToList();
            if (impassableEntities.Any())
            {
                // A collision has occurred
                var collidee = impassableEntities.First(); // Not sure if just getting first here will work forever...
                var damage = Has<ProjectileDamageComponent>(movingEntity) ? Get<ProjectileDamageComponent>(movingEntity).Damage : 0;
                Send(impassableEntities.First(), new DamageMessage(damage));

                var collisionBehaviour = Has<OnCollisionComponent>(movingEntity) ? Get<OnCollisionComponent>(movingEntity).Behaviour : CollisionBehaviour.Stop;
                switch (collisionBehaviour)
                {
                    case CollisionBehaviour.Stop:
                        RemoveTempProjectileComponents(movingEntity);
                        break;
                    case CollisionBehaviour.StopAndPush:
                        RemoveTempProjectileComponents(movingEntity);
                        Send(collidee, new PushMessage(direction));
                        break;
                    case CollisionBehaviour.DestroySelf:
                        Destroy(movingEntity);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return false;
            }
        }
        else
        {
            // Must be at edge of map because there should at least be a GroundTile at every valid coord
            Destroy(movingEntity);
            return false;
        }

        return true;
    }

    private void RemoveTempProjectileComponents(Entity entity)
    {
        Remove<SpeedComponent>(entity);
        Remove<ProjectileDamageComponent>(entity);
        Remove<ProjectileMoveRateComponent>(entity);
        Remove<MovingInDirectionComponent>(entity);
        Remove<OnCollisionComponent>(entity);
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