using System;
using MoonTools.ECS;
using Enamel.Components;

namespace Enamel.Systems;

public class GridToScreenCoordSystem : MoonTools.ECS.System
{
    private Filter GridCoordFilter { get; }
    private readonly int _xOffset;
    private readonly int _yOffset;

    public GridToScreenCoordSystem(World world, int xOffset, int yOffset) : base(world)
    {
        GridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Build();
        _xOffset = xOffset;
        _yOffset = yOffset;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in GridCoordFilter.Entities)
        {
            var (x, y) = Get<GridCoordComponent>(entity);
            var screenCoords = Utils.GridToScreenCoords(x, y);
            Set(entity, new ScreenPositionComponent(screenCoords.X + _xOffset, screenCoords.Y + _yOffset));
        }
    }
}