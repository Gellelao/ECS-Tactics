using System;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;

namespace Enamel.Systems;

public class UnitSelectionSystem : MoonTools.ECS.System
{
    private Filter SelectableCoordFilter { get; }
    private Filter SelectedFilter { get; }

    public UnitSelectionSystem(World world) : base(world)
    {
        SelectableCoordFilter = FilterBuilder
            .Include<SelectableFlag>()
            .Include<GridCoordComponent>()
            .Exclude<MovementPreviewFlag>()
            .Build();
        SelectedFilter = FilterBuilder.Include<SelectedFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in SelectableCoordFilter.Entities)
        {
            if (SomeMessageWithEntity<SelectMessage>(entity))
            {
                foreach (var selectedEntity in SelectedFilter.Entities){
                    Remove<SelectedFlag>(selectedEntity);
                }
                Set(entity, new SelectedFlag());
            }
        }
    }
}