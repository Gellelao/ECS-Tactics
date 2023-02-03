using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MoonTools.ECS;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Enamel.Components;
using Enamel.Components.Messages;

namespace Enamel.Systems
{
    public class InputSystem : MoonTools.ECS.System
    {
        private Filter SelectableFilter { get; }
        private Filter GridCoordFilter { get; }
	    private MouseState _mousePrevious;
        private readonly int _upscaleFactor;
        private readonly int _tileWidth;
        private readonly int _tileHeight;
        private readonly int _xOffset;
        private readonly int _yOffset;

        public InputSystem(World world, int upscaleFactor, int tileWidth, int tileHeight, int xOffset, int yOffset) : base(world)
        {
            SelectableFilter = FilterBuilder.Include<SelectableComponent>().Build();
            GridCoordFilter = FilterBuilder.Include<GridCoordComponent>().Build();
            _upscaleFactor = upscaleFactor;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _xOffset = xOffset;
            _yOffset = yOffset;
        }

        public override void Update(TimeSpan delta)
        {
            var mouseCurrent = Mouse.GetState();
            OnUpdate(mouseCurrent.X, mouseCurrent.Y);
            if (mouseCurrent.LeftButton == ButtonState.Released && _mousePrevious.LeftButton == ButtonState.Pressed)
            {
                OnLeftButtonRelease(mouseCurrent.X, mouseCurrent.Y);
            }
            _mousePrevious = mouseCurrent;
        }

        private void OnUpdate(int mouseX, int mouseY)
        {
            var gridCoords = ScreenToGridCoords(mouseX/_upscaleFactor, mouseY/_upscaleFactor);
            foreach (var entity in GridCoordFilter.Entities)
            {
                var (x, y) = Get<GridCoordComponent>(entity);
                if((int)gridCoords.X == x && (int)gridCoords.Y == y)
                {
                    Send(new Highlight(entity));
                }
            }
        }

        private void OnLeftButtonRelease(int mouseX, int mouseY){
            // Will want to check button clicks before grid coords, in case there is a popup window over the grid
            var clickedCoords = ScreenToGridCoords(mouseX/_upscaleFactor, mouseY/_upscaleFactor);
            foreach (var entity in SelectableFilter.Entities.Intersect(GridCoordFilter.Entities))
            {
                var (x, y) = Get<GridCoordComponent>(entity);
                if((int)clickedCoords.X == x && (int)clickedCoords.Y == y)
                {
                    Send(new Select(entity));
                }
            }
        }

        private Vector2 ScreenToGridCoords(int mouseX, int mouseY)
        {
            float mouseFloatX = mouseX - (_tileWidth/2) - _xOffset;
            float mouseFloatY = mouseY - _yOffset;
            float tileWidthFloat = _tileWidth;
            float tileHeightFloat = _tileHeight;

            var gridX = mouseFloatY / tileHeightFloat + mouseFloatX / tileWidthFloat;
            var gridY = mouseFloatY / tileHeightFloat - mouseFloatX / tileWidthFloat;
            
            return new Vector2((int)Math.Floor(gridX), (int)Math.Floor(gridY));
        }
    }
}