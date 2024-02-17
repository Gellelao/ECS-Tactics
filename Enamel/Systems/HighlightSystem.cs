using System;
using Enamel.Components;
using Enamel.Components.UI;
using Enamel.Systems.UI;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

/// <summary>
/// This is for programmatic highlighting - for entities that use a different frame when hovered, <see cref="ToggleFrameSystem"/>
/// </summary>
public class HighlightSystem : MoonTools.ECS.System
{
    private readonly ScreenUtils _screenUtils;
    private Filter HighlightedFilter { get; }
    private Filter ClickableUiFilter { get; }
    private Filter SelectableGridCoordFilter { get; }

    public HighlightSystem(World world, ScreenUtils screenUtils) : base(world)
    {
        _screenUtils = screenUtils;
        HighlightedFilter = FilterBuilder.Include<HighlightedFlag>().Build();
        ClickableUiFilter = FilterBuilder
            .Include<OnClickComponent>()
            .Exclude<DisabledFlag>()
            .Exclude<ToggleFrameOnMouseHoverComponent>() // Don't want to double-highlight
            .Build();
        SelectableGridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Exclude<DisabledFlag>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        // Clear existing hovers
        foreach (var selectedEntity in HighlightedFilter.Entities)
        {
            Remove<HighlightedFlag>(selectedEntity);
        }

        if (Some<BeingDraggedFlag>())
        {
            // Don't highlight stuff while dragging is happening
            return;
        }

        // Button hovers
        foreach (var button in ClickableUiFilter.Entities)
        {
            if (_screenUtils.MouseOverEntity(button))
            {
                Set(button, new HighlightedFlag());
            }
        }

        // Unit hovers
        var gridCoords = _screenUtils.MouseToGridCoords();
        foreach (var entity in SelectableGridCoordFilter.Entities)
        {
            var (x, y) = Get<GridCoordComponent>(entity);
            if ((int) gridCoords.X == x && (int) gridCoords.Y == y)
            {
                Set(entity, new HighlightedFlag());
            }
        }
    }
}