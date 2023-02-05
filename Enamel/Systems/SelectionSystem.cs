using System;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;

namespace Enamel.Systems;

public class SelectionSystem : MoonTools.ECS.System
{
    private Filter SelectableCoordFilter { get; }
    private Filter SelectedFilter { get; }

    public SelectionSystem(World world) : base(world)
    {
        SelectableCoordFilter = FilterBuilder
            .Include<SelectableComponent>()
            .Include<GridCoordComponent>()
            .Build();
        SelectedFilter = FilterBuilder.Include<SelectedComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in SelectableCoordFilter.Entities)
        {
            if (SomeMessageWithEntity<Select>(entity))
            {
                foreach (var selectedEntity in SelectedFilter.Entities){
                    Remove<SelectedComponent>(selectedEntity);
                }
                Set(entity, new SelectedComponent());
            }
        }
    }
}