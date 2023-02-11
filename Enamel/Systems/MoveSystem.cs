using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Messages;
using Microsoft.Xna.Framework;
using MoonTools.ECS;

namespace Enamel.Systems;

public class MoveSystem : MoonTools.ECS.System
{
    private Filter MovePreviewFilter { get; }
    private Filter SelectedUnitFilter { get; }
    private Filter MovingUnitsFilter { get; }
    
    public MoveSystem(World world) : base(world)
    {
        MovePreviewFilter = FilterBuilder
            .Include<MovementPreviewFlag>()
            .Build();
        SelectedUnitFilter = FilterBuilder
            .Include<SelectedFlag>()
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
            foreach (var selectedEntity in SelectedUnitFilter.Entities)
            {
                // Could store the grid coords on the MovingToPositionComponent as well, so we know what GridCoord to give it once movement completes
                Set(selectedEntity, new MovingToPositionComponent(targetScreenPosition.X, targetScreenPosition.Y, targetGridPosition.X, targetGridPosition.Y));
                Remove<GridCoordComponent>(selectedEntity);
            }
        }
        
        // Shift entities that are already moving
        foreach (var entity in MovingUnitsFilter.Entities)
        {
            var positionComponent = Get<PositionComponent>(entity);
            var targetPosition = Get<MovingToPositionComponent>(entity);
            var newPosition = MoveTowards(positionComponent, targetPosition, delta);
            // If at destination, put the gridComponent back on at the target grid coords
            var newPositionVector = newPosition.ToVector;
            if ((int)Math.Round(newPositionVector.X) == targetPosition.ScreenX && (int)Math.Round(newPositionVector.Y) == targetPosition.ScreenY)
            {
                Set(entity, new GridCoordComponent(targetPosition.GridX, targetPosition.GridY));
                Remove<MovingToPositionComponent>(entity);
            }
            // Otherwise, move the screenpos
            else
            {
                Set(entity, newPosition);
            }
        }
    }

    private PositionComponent MoveTowards(PositionComponent current, MovingToPositionComponent target, TimeSpan deltaTime)
    {
        var currentVector = current.ToVector;
        var x = currentVector.X;
        var y = currentVector.Y;
        if (x < target.ScreenX) x += (float)(2 * Constants.MoveSpeed*deltaTime.TotalSeconds);
        if (x > target.ScreenX) x -= (float)(2 * Constants.MoveSpeed*deltaTime.TotalSeconds);
        if (y < target.ScreenY) y += (float)(Constants.MoveSpeed*deltaTime.TotalSeconds);
        if (y > target.ScreenY) y -= (float)(Constants.MoveSpeed*deltaTime.TotalSeconds);
        return new PositionComponent(x, y);
    }
}