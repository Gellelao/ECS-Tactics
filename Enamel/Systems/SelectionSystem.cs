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
            .Include<SelectableFlag>()
            .Include<GridCoordComponent>()
            .Build();
        SelectedFilter = FilterBuilder.Include<SelectedFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in SelectableCoordFilter.Entities)
        {
            if (SomeMessageWithEntity<Select>(entity))
            {
                foreach (var selectedEntity in SelectedFilter.Entities){
                    Remove<SelectedFlag>(selectedEntity);
                }
                Set(entity, new SelectedFlag());
            }
        }
    }
}