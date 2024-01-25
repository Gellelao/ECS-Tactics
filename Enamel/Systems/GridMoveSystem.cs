using System;
using Enamel.Components;
using Enamel.Components.Spells.SpawnedEntities;
using Microsoft.Xna.Framework;
using MoonTools.ECS;
using static Enamel.Utils.Utils;

namespace Enamel.Systems;

public class GridMoveSystem : MoonTools.ECS.System
{
    private Filter MovingUnitsFilter { get; }
    private readonly int _xOffset;
    private readonly int _yOffset;
    
    public GridMoveSystem(World world, int xOffset, int yOffset) : base(world)
    {
        MovingUnitsFilter = FilterBuilder
            .Include<MovingToGridCoordComponent>()
            .Build();
        _xOffset = xOffset;
        _yOffset = yOffset;
    }

    public override void Update(TimeSpan delta)
    {
        // Shift entities that are already moving
        foreach (var entity in MovingUnitsFilter.Entities)
        {
            var positionComponent = Get<ScreenPositionComponent>(entity);
            var targetPosition = Get<MovingToGridCoordComponent>(entity);
            var speed = Has<SpeedComponent>(entity) ? Get<SpeedComponent>(entity).Speed : Constants.DEFAULT_WALK_SPEED;

            var targetScreenPos = GridToScreenCoords(targetPosition.GridX, targetPosition.GridY);
            targetScreenPos.X += _xOffset;
            targetScreenPos.Y += _yOffset;

            var newPosition = MoveTowards(positionComponent, targetScreenPos, speed, delta);
            var newPositionVector = newPosition.ToVector;
            // Fast moving entities need a more lenient threshold to tell if they are at destination
            var threshold = speed < 80 ? 1 : 4;
            // If at destination, reapply the gridComponent with the target grid coords
            if (Math.Abs(Math.Round(newPositionVector.X) - targetScreenPos.X) <= threshold &&
                Math.Abs(Math.Round(newPositionVector.Y) - targetScreenPos.Y) <= threshold)
            {
                Set(entity, new GridCoordComponent(targetPosition.GridX, targetPosition.GridY));
                Remove<MovingToGridCoordComponent>(entity);

                // Little bit of jank here to exclude player characters currently travelling as projectiles...
                // and playerControlled units who are pushed...
                if (Has<RemainingMovesComponent>(entity) && !Has<MovingInDirectionComponent>(entity) && Has<SelectedFlag>(entity))
                {
                    UpdateRemainingMoves(entity);
                }
            }
            // Otherwise, move the screenpos
            else
            {
                Set(entity, newPosition);
            }
        }
    }

    private void UpdateRemainingMoves(Entity entity)
    {
        var remainingMoves = Get<RemainingMovesComponent>(entity).RemainingMoves - 1;
        Set(entity, new RemainingMovesComponent(remainingMoves));
    }

    // Replace this with a util that uses the code from the ScreenMoveSystem?????
    private ScreenPositionComponent MoveTowards(ScreenPositionComponent current, Vector2 target, int moveSpeed, TimeSpan deltaTime)
    {
        var currentVector = current.ToVector;
        var x = currentVector.X;
        var y = currentVector.Y;
        if (x < target.X) x += (float)(2 * moveSpeed*deltaTime.TotalSeconds);
        if (x > target.X) x -= (float)(2 * moveSpeed*deltaTime.TotalSeconds);
        if (y < target.Y) y += (float)(moveSpeed*deltaTime.TotalSeconds);
        if (y > target.Y) y -= (float)(moveSpeed*deltaTime.TotalSeconds);
        return new ScreenPositionComponent(x, y);
    }
}