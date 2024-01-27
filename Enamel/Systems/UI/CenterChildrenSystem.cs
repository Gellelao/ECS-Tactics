using System;
using System.Collections.Generic;
using System.Linq;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using MoonTools.ECS;

namespace Enamel.Systems.UI;

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
            var childrenByOrder = new Dictionary<int, List<Entity>>();
            
            foreach (var child in children)
            {
                var order = Get<OrderComponent>(child).Order;
                var width = Get<DimensionsComponent>(child).Width;

                if (!childrenByOrder.TryGetValue(order, out List<Entity>? entities))
                {
                    entities = [];
                    childrenByOrder[order] = entities;

                    // If to children have same order we don't want to add width for both of them, so just do it in here
                    totalWidthOfChildren += width;
                }

                entities.Add(child);
            }
            
            var orderedChildren = childrenByOrder
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value)
                .ToList();

            var center = xBounds / 2;
            var totalBufferWidth = buffer * (orderedChildren.Count - 1);
            var halfChildrenWidth = (totalWidthOfChildren + totalBufferWidth) / 2;

            var xOrigin = center - halfChildrenWidth;

            foreach (var childList in orderedChildren)
            {
                // If there is more than 1 child, just extend xOrigin by the width of the first child for now
                var width = Get<DimensionsComponent>(childList.First()).Width;
                foreach (var child in childList){
                    var position = Get<ScreenPositionComponent>(child);
                    Set(child, new ScreenPositionComponent(xOrigin, position.Y));
                }
                xOrigin += width;
                xOrigin += buffer;
            }
        }
    }
}