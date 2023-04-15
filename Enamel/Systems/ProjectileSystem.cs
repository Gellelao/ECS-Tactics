﻿using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Enums;
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

        var direction = Get<MovingInDirectionComponent>(movingEntity).Direction;

        Send(movingEntity, new PushMessage(direction, false));
    }
}