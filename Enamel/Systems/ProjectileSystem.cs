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

        var shouldMoveEntity = HandleCollisionAndOutOfBounds(movingEntity, gridX, gridY);
        if (!shouldMoveEntity) return;

        _world.Set(movingEntity, new MovingToCoordComponent(gridX, gridY));
        Remove<GridCoordComponent>(movingEntity);
    }

    private bool HandleCollisionAndOutOfBounds(Entity movingEntity, int candidateX, int candidateY)
    {
        // check the oncollisioncomponent
        // if destroy, do as already here
        // if not, take off the projectile components and leave the entity where t is

        // Temporary speedComponents are added to units moved by spells, this is just tidying that up
        if(Has<SpeedComponent>(movingEntity))
        {
            //TODO : move this to where the projectile actually stops
            //Remove<SpeedComponent>(entity);
        }

        var damage = Has<ProjectileDamageComponent>(movingEntity) ? Get<ProjectileDamageComponent>(movingEntity).Damage : 0;
        var entitiesAtLocation = GetEntitiesAtCoords(candidateX, candidateY);
        // It actually looks quite good with projectiles travelling past the edge of the screen, but need to destroy them eventually, so for now just destroy once out of bounds of the map
        entitiesAtLocation.Remove(movingEntity);
        if (entitiesAtLocation.Any())
        {
            var impassableEntities = entitiesAtLocation.Where(e => Has<ImpassableFlag>(e)).ToList();
            if (impassableEntities.Any())
            {
                Send(impassableEntities.First(), new DamageMessage(damage));
                Destroy(movingEntity);
                return false;
            }
        }
        else
        {
            // Must be at edge of map because there should at least be a GroundTile at every valid coord
            Console.WriteLine($"Destroying projectile because it reached {candidateX},{candidateY}");
            Destroy(movingEntity);
            return false;
        }

        return true;
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