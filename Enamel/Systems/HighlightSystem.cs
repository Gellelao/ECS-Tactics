using System;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;

namespace Enamel.Systems;

public class HighlightSystem : MoonTools.ECS.System
{
    private Filter HighlightedFilter { get; }
    private Filter GridCoordComponentFilter { get; }

    public HighlightSystem(World world) : base(world)
    {
        HighlightedFilter = FilterBuilder.Include<HighlightedFlag>().Build();
        GridCoordComponentFilter = FilterBuilder.Include<GridCoordComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var selectedEntity in HighlightedFilter.Entities){
            Remove<HighlightedFlag>(selectedEntity);
        }

        foreach (var entity in GridCoordComponentFilter.Entities)
        {
            if (SomeMessageWithEntity<HighlightMessage>(entity))
            {
                Set(entity, new HighlightedFlag());
            }
        }
    }
}