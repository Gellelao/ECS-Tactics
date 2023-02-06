using System;
using Enamel.Components;
using Enamel.Components.Messages;
using MoonTools.ECS;

namespace Enamel.Systems;

public class MoveSystem : MoonTools.ECS.System
{
    private Filter MovePreviewFilter { get; }
    private Filter SelectedUnitFilter { get; }
    
    public MoveSystem(World world) : base(world)
    {
        MovePreviewFilter = FilterBuilder
            .Include<MovementPreviewFlag>()
            .Build();
        SelectedUnitFilter = FilterBuilder
            .Include<SelectedFlag>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in MovePreviewFilter.Entities)
        {
            if (SomeMessageWithEntity<SelectMessage>(entity))
            {
                var gridCoordComponent = Get<GridCoordComponent>(entity);
                foreach (var selectedEntity in SelectedUnitFilter.Entities){
                    Set(selectedEntity, gridCoordComponent);
                }
            }
        }
    }
}