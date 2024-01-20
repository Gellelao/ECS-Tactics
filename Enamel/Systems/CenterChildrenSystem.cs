using System;
using System.Collections.Generic;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using MoonTools.ECS;

namespace Enamel.Systems;

public class CenterChildrenSystem : MoonTools.ECS.System
{
    private Filter CenterChildrenFilter { get; }

    public CenterChildrenSystem(World world) : base(world)
    {
        CenterChildrenFilter = FilterBuilder.Include<CenterChildrenComponent>().Build();
    }


    public override void Update(TimeSpan delta)
    {
        foreach (var parent in CenterChildrenFilter.Entities)
        {
            var buffer = Get<CenterChildrenComponent>(parent).BufferPixels;
            
            var children = OutRelations<IsParentRelation>(parent);
            
            var xBounds = Has<DimensionsComponent>(parent)
                ? Get<DimensionsComponent>(parent).Width
                : Constants.PIXEL_SCREEN_WIDTH;

            var totalWidthOfChildren = 0;
            var childrenByOrder = new Dictionary<int, Entity>();
            
            foreach (var child in children)
            {
                var order = Get<OrderComponent>(child).Order;
                var width = Get<DimensionsComponent>(child).Width;

                childrenByOrder.Add(order, child);
                totalWidthOfChildren += width;
            }
            
            var orderedChildren = childrenByOrder
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value)
                .ToList();

            var center = xBounds / 2;
            var totalBufferWidth = buffer * (orderedChildren.Count - 1);
            var halfChildrenWidth = (totalWidthOfChildren + totalBufferWidth) / 2;

            var xOrigin = center - halfChildrenWidth;

            foreach (var child in orderedChildren)
            {
                var position = Get<ScreenPositionComponent>(child);
                var width = Get<DimensionsComponent>(child).Width;
                Set(child, new ScreenPositionComponent(xOrigin, position.Y));
                xOrigin += width;
                xOrigin += buffer;
            }
        }
    }
}