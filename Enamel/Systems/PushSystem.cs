using System;
using System.Collections.Generic;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class PushSystem : MoonTools.ECS.System
{
    private readonly World _world;
    private Filter GridCoordFilter { get; }

    public PushSystem(World world) : base(world)
    {
        _world = world;
        GridCoordFilter = FilterBuilder.Include<GridCoordComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        var seenEntityIds = new List<int>();
        Console.WriteLine("PushSystem updating");
        foreach (var entity in GridCoordFilter.Entities)
        {
            if (seenEntityIds.Contains(entity.ID)) continue;
            seenEntityIds.Add(entity.ID);
            if (entity.ID == 4)
            {
                Console.WriteLine($"Getting push messages for entity {entity.ID}");
            }
            var pushMessages = ReadMessagesWithEntity<PushMessage>(entity);

            var count = 0;
            foreach (var pushMessage in pushMessages)
            {
                count++;
                Console.WriteLine($"Found a push message for entity {entity.ID} (#{count})");
                Push(entity, pushMessage.Direction, pushMessage.EntityMustBePushable);
            }
        }
    }

    private void Push(Entity entity, Direction direction, bool entityMustBePushable)
    {
        Console.WriteLine($"Entity {entity.ID} being pushed");
        // Early return if this push requires the entity to have the pushable flag, and it does not
        if (entityMustBePushable && !Has<PushableFlag>(entity)) return;

        var (gridX, gridY) = Get<GridCoordComponent>(entity);

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

        var shouldMoveEntity = HandleCollisionAndOutOfBounds(entity, direction, gridX, gridY);
        if (!shouldMoveEntity)
        {
            Console.WriteLine($"Should not move entity {entity.ID}, returning");
            return;
        }

        Console.WriteLine($"Moving entity to {gridX},{gridY}");
        _world.Set(entity, new MovingToCoordComponent(gridX, gridY));
        Remove<GridCoordComponent>(entity);
    }

    private bool HandleCollisionAndOutOfBounds(Entity movingEntity, Direction direction, int candidateX, int candidateY)
    {
        var entitiesAtLocation = GetEntitiesAtCoords(candidateX, candidateY);

        if (entitiesAtLocation.Any())
        {
            var impassableEntities = entitiesAtLocation.Where(e => Has<ImpassableFlag>(e)).ToList();
            if (impassableEntities.Any())
            {
                Console.WriteLine($"Collision at candidate pos {candidateX},{candidateY}");
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
                        // Recursion Alert!
                        // Recursion Alert!
                        Console.WriteLine($"Recursively calling push on entity {collidee.ID}");
                        Push(collidee, direction, true);
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