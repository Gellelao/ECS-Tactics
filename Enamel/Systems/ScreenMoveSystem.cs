using System;
using Enamel.Components;
using Microsoft.Xna.Framework;
using MoonTools.ECS;

namespace Enamel.Systems;

public class ScreenMoveSystem : MoonTools.ECS.System
{
    private const double TOLERANCE = 0.001;

    private Filter MovingFilter { get; }

    public ScreenMoveSystem(World world) : base(world)
    {
        MovingFilter = FilterBuilder
            .Include<MovingToScreenPositionComponent>()
            .Include<ScreenPositionComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in MovingFilter.Entities)
        {
            var currentPosition = Get<ScreenPositionComponent>(entity);
            var targetPosition = Get<MovingToScreenPositionComponent>(entity);
            var updatedPosition = UpdatePosition(
                currentPosition.ToVector,
                targetPosition.X,
                targetPosition.Y,
                targetPosition.MoveSpeed,
                delta
            );

            Set(entity, new ScreenPositionComponent(updatedPosition.X, updatedPosition.Y));

            if (Math.Abs(updatedPosition.X - targetPosition.X) < TOLERANCE && Math.Abs(updatedPosition.Y - targetPosition.Y) < TOLERANCE)
            {
                Remove<MovingToScreenPositionComponent>(entity);
            }
        }
    }

    private (float X, float Y) UpdatePosition(
        Vector2 currentPosition,
        float targetX,
        float targetY,
        float moveSpeed,
        TimeSpan delta
    )
    {
        // Calculate the direction vector
        float directionX = targetX - currentPosition.X;
        float directionY = targetY - currentPosition.Y;
        float distance = (float) Math.Sqrt(directionX * directionX + directionY * directionY);

        // Normalize the direction
        if (distance > 0)
        {
            directionX /= distance;
            directionY /= distance;
        }

        // Calculate the distance to move based on speed and time delta
        float moveDistance = moveSpeed * (float) delta.TotalSeconds;

        // Calculate the new position
        float newX, newY;

        // If the moveDistance is greater than or equal to the distance to the target, set position to target
        if (moveDistance >= distance)
        {
            newX = targetX;
            newY = targetY;
        }
        else
        {
            // Move towards the target
            newX = currentPosition.X + directionX * moveDistance;
            newY = currentPosition.Y + directionY * moveDistance;
        }

        return (newX, newY);
    }
}