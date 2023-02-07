using System;
using MoonTools.ECS;
using Enamel.Components;
using Microsoft.Xna.Framework;
using MoonTools.Structs;

namespace Enamel.Systems;

public class GridToScreenCoordSystem : MoonTools.ECS.System
{
    private Filter GridCoordFilter { get; }
    private readonly int _tileWidth;
    private readonly int _tileHeight;
    private readonly int _xOffset;
    private readonly int _yOffset;

    public GridToScreenCoordSystem(World world, int tileWidth, int tileHeight, int xOffset, int yOffset) : base(world)
    {
        GridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Build();
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
        _xOffset = xOffset;
        _yOffset = yOffset;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in GridCoordFilter.Entities)
        {
            var (x, y) = Get<GridCoordComponent>(entity);
            var screenCoords = GridToScreenCoords(x, y);
            Set(entity, new PositionComponent(screenCoords.X + _xOffset, screenCoords.Y + _yOffset));
        }
    }

    private Vector2 GridToScreenCoords(int gridX, int gridY){
        float screenX = _tileWidth * gridX / 2 -
                      _tileWidth * gridY / 2;
        float screenY = _tileHeight * gridX / 2 +
                      _tileHeight * gridY / 2;
        return new Vector2(screenX, screenY);
    }
}