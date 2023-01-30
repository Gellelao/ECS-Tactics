using System;
using MoonTools.ECS;
using Enamel.Components;
using Microsoft.Xna.Framework;

namespace Enamel.Systems
{
    public class GridToScreenCoordSystem : MoonTools.ECS.System
    {
        private Filter GridCoordFilter { get; }
        private int _tileWidth;
        private int _tileHeight;
        private int _mapWidth;
        private int _mapHeight;
        private readonly int _xOffset;
        private readonly int _yOffset;

        public GridToScreenCoordSystem(World world, int tileWidth, int tileHeight, int mapWidth, int mapHeight, int xOffset, int yOffset) : base(world)
        {
            GridCoordFilter = FilterBuilder
                            .Include<GridCoordComponent>()
                            .Build();
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _xOffset = xOffset;
            _yOffset = yOffset;
        }

        public override void Update(TimeSpan delta)
        {
            foreach (var entity in GridCoordFilter.Entities)
            {
                var gridCoordComponent = Get<GridCoordComponent>(entity);
                var screenCoords = GridToScreenCoords(gridCoordComponent.X, gridCoordComponent.Y);
                Set(entity, new PositionComponent((int)Math.Round(screenCoords.X) + _xOffset, (int)Math.Round(screenCoords.Y) + _yOffset));
            }
        }

        private Vector2 GridToScreenCoords(int gridX, int gridY){
            var screenX = _tileWidth * gridX / 2 -
                          _tileWidth * gridY / 2;
            var screenY = _tileHeight * gridX / 2 +
                          _tileHeight * gridY / 2;
            return new Vector2(screenX, screenY);
        }
    }
}