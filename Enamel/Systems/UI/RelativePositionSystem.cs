using System;
using System.Collections.Generic;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using MoonTools.ECS;

namespace Enamel.Systems;

public class RelativePositionSystem : MoonTools.ECS.System
{
    private Filter RelativePositionFilter { get; }

    public RelativePositionSystem(World world) : base(world)
    {
        RelativePositionFilter = FilterBuilder.Include<RelativePositionComponent>().Build();
    }


    public override void Update(TimeSpan delta)
    {
        foreach (var child in RelativePositionFilter.Entities)
        {
            var relativePos = Get<RelativePositionComponent>(child);
            var parents = InRelations<IsParentRelation>(child);
            var count = 0;
            foreach (var parent in parents)
            {
                count++;
                if (count > 1) throw new Exception("Relative position must not have more than 1 parent");

                var parentPos = Get<ScreenPositionComponent>(parent);
                Set(child, new ScreenPositionComponent(parentPos.X + relativePos.X, parentPos.Y + relativePos.Y));
            }
        }
    }
}