using System;
using System.Collections.Generic;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Components.TempComponents;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class PushSystem : MoonTools.ECS.System
{
    private readonly World _world;
    private Filter PushableFilter { get; }

    public PushSystem(World world) : base(world)
    {
        _world = world;
        PushableFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Include<BeingPushedComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in PushableFilter.Entities)
        {
            var (direction, mustBePushable) = Get<BeingPushedComponent>(entity);
            Push(entity, direction, mustBePushable);
        }
        // What happens here is the pusher may start with its id at the beginning of the GridCoordFilter entities.
        // Then in the process of pushing, its id may be duplicated in the GridCoordFilter entities.
        //   The specific cause seems to be the IndexableSet doing weird stuff when Remove is called, when the GridCoordComponent is removed from the pushee at the end of Push
        // This causes the pusher to be iterated over twice

        // So we either keep a list of seen entities and only check those that haven't been seen,
        // take a copy of the entities when this method is entered, and only loop over those,
        // or delete the message once it has been read once, so that on the second enumeration nothing happens
        // or avoid the issue altogether by not removing a GridCoordComponent during iteration of the GridCoordFilter
        // var entitiesToPush = new List<(Entity Entity, BeingPushedComponent Message)>();
        // foreach (var entity in PushableFilter.Entities)
        // {
        //     var pushMessages = ReadMessagesWithEntity<BeingPushedComponent>(entity);
        //
        //     foreach (var pushMessage in pushMessages)
        //     {
        //         entitiesToPush.Add((entity, pushMessage));
        //     }
        // }
        //
        // entitiesToPush.ForEach(tuple => Push(tuple.Entity, tuple.Message.Direction, tuple.Message.EntityMustBePushable));
    }

    private void Push(Entity entity, Direction direction, bool entityMustBePushable)
    {
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
        if (!shouldMoveEntity) return;

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
                // A collision has occurred
                var collidee = impassableEntities.First(); // Not sure if just getting first here will work forever...
                var damage = Has<ProjectileDamageComponent>(movingEntity) ? Get<ProjectileDamageComponent>(movingEntity).Damage : 0;
                Set(collidee, new TakingDamageComponent(damage));

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
        foreach (var entity in PushableFilter.Entities)
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