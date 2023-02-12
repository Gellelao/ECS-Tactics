using System;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;

namespace Enamel.Systems;

public class HighlightSystem : MoonTools.ECS.System
{
    private Filter HighlightedFilter { get; }
    private Filter TextureIndexFilter { get; }

    public HighlightSystem(World world) : base(world)
    {
        HighlightedFilter = FilterBuilder.Include<HighlightedFlag>().Build();
        TextureIndexFilter = FilterBuilder.Include<TextureIndexComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var selectedEntity in HighlightedFilter.Entities){
            Remove<HighlightedFlag>(selectedEntity);
        }

        foreach (var entity in TextureIndexFilter.Entities)
        {
            if (SomeMessageWithEntity<HighlightMessage>(entity))
            {
                Set(entity, new HighlightedFlag());
            }
        }
    }
}