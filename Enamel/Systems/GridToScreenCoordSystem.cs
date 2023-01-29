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

        public GridToScreenCoordSystem(World world, int tileWidth, int tileHeight, int mapWidth, int mapHeight) : base(world)
        {
            GridCoordFilter = FilterBuilder
                            .Include<GridCoordComponent>()
                            .Build();
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
        }

        public override void Update(TimeSpan delta)
        {
            foreach (var entity in GridCoordFilter.Entities)
            {
                var gridCoordComponent = Get<GridCoordComponent>(entity);
                var screenCoords = GridToScreenCoords(gridCoordComponent.X, gridCoordComponent.Y);
                Set(entity, new PositionComponent((int)Math.Round(screenCoords.X), (int)Math.Round(screenCoords.Y)));
            }
        }

        private Vector2 GridToScreenCoords(int gridX, int gridY){
            var screenX = _tileWidth * gridX / 2 +
                          _tileWidth * _mapHeight / 2 -
                          _tileWidth * gridY / 2;
            var screenY = (_mapHeight - gridY - 1) * _tileHeight / 2 +
                          _mapWidth * _tileHeight / 2  -
                          gridX * _tileHeight / 2;
            return new Vector2(screenX, screenY);
        }
    }
}