using System;
using Enamel.Components;
using Enamel.Components.Messages;
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
            if (SomeMessageWithEntity<SelectMessage>(entity))
            {
                var positionComponent = Get<PositionComponent>(entity);
                foreach (var selectedEntity in SelectedUnitFilter.Entities){
                    Set(selectedEntity, new MovingToPositionComponent(positionComponent.X, positionComponent.Y));
                    Remove<GridCoordComponent>(selectedEntity);
                }
            }
        }
        
        // Shift entities that are already moving
        foreach (var entity in MovingUnitsFilter.Entities)
        {
            var positionComponent = Get<PositionComponent>(entity);
            var targetPosition = Get<MovingToPositionComponent>(entity);
            var newPosition = MoveTowards(positionComponent, targetPosition, delta);
            Set(entity, newPosition);
        }
    }

    private PositionComponent MoveTowards(PositionComponent current, MovingToPositionComponent target, TimeSpan deltaTime)
    {
        float x = current.X;
        float y = current.Y;
        if (x < target.X) x += (float)(2 * Constants.MoveSpeed*deltaTime.TotalSeconds);
        if (x > target.X) x -= (float)(2 * Constants.MoveSpeed*deltaTime.TotalSeconds);
        if (y < target.Y) y += (float)(Constants.MoveSpeed*deltaTime.TotalSeconds);
        if (y > target.Y) y -= (float)(Constants.MoveSpeed*deltaTime.TotalSeconds);
        return new PositionComponent(x, y);
    }
}