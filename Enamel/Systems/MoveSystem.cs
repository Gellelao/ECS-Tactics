using System;
using Enamel.Components;
using Enamel.Components.Messages;
using MoonTools.ECS;

namespace Enamel.Systems;

public class MoveSystem : MoonTools.ECS.System
{
    private Filter MovePreviewFilter { get; }
    private Filter MovingUnitsFilter { get; }
    
    public MoveSystem(World world) : base(world)
    {
        MovePreviewFilter = FilterBuilder
            .Include<MovementPreviewFlag>()
            .Build();
        MovingUnitsFilter = FilterBuilder
            .Include<MovingToPositionComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        // Start moving selected entity if a MovePreview was selected
        foreach (var entity in MovePreviewFilter.Entities)
        {
            if (!SomeMessageWithEntity<SelectMessage>(entity)) continue;
            
            var targetScreenPosition = Get<PositionComponent>(entity);
            var targetGridPosition = Get<GridCoordComponent>(entity);
            var selectedEntity = GetSingletonEntity<SelectedFlag>();
            Set(selectedEntity, new MovingToPositionComponent(targetScreenPosition.X, targetScreenPosition.Y, targetGridPosition.X, targetGridPosition.Y));
            Remove<GridCoordComponent>(selectedEntity);
        }
        
        // Shift entities that are already moving
        foreach (var entity in MovingUnitsFilter.Entities)
        {
            var positionComponent = Get<PositionComponent>(entity);
            var targetPosition = Get<MovingToPositionComponent>(entity);
            var speed = Has<SpeedComponent>(entity) ? Get<SpeedComponent>(entity).Speed : Constants.DEFAULT_MOVE_SPEED;
            var newPosition = MoveTowards(positionComponent, targetPosition, speed, delta);
            // If at destination, reapply the gridComponent with the target grid coords
            var newPositionVector = newPosition.ToVector;
            // Fast moving entities need a more lenient threshold to tell if they are at destination
            var threshold = speed < 80 ? 1 : 2;
            if (Math.Abs(Math.Round(newPositionVector.X) - targetPosition.ScreenX) <= threshold &&
                Math.Abs(Math.Round(newPositionVector.Y) - targetPosition.ScreenY) <= threshold)
            {
                Set(entity, new GridCoordComponent(targetPosition.GridX, targetPosition.GridY));
                Remove<MovingToPositionComponent>(entity);

                if (Has<RemainingMovesComponent>(entity))
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

        if (remainingMoves > 0)
        {
            // Danger, issuing a select message outside of the InputSystem!!
            // This is so the selectionPreview system knows to display the preview again
            Send(new SelectMessage(entity));
        }
    }

    private PositionComponent MoveTowards(PositionComponent current, MovingToPositionComponent target, int moveSpeed, TimeSpan deltaTime)
    {
        var currentVector = current.ToVector;
        var x = currentVector.X;
        var y = currentVector.Y;
        if (x < target.ScreenX) x += (float)(2 * moveSpeed*deltaTime.TotalSeconds);
        if (x > target.ScreenX) x -= (float)(2 * moveSpeed*deltaTime.TotalSeconds);
        if (y < target.ScreenY) y += (float)(moveSpeed*deltaTime.TotalSeconds);
        if (y > target.ScreenY) y -= (float)(moveSpeed*deltaTime.TotalSeconds);
        return new PositionComponent(x, y);
    }
}