using System;
using MoonTools.ECS;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Enamel.Systems
{
    public class InputSystem : MoonTools.ECS.System
    {
	    private MouseState mousePrevious = new MouseState();
        private readonly int _upscaleFactor;
        private readonly int _tileWidth;
        private readonly int _tileHeight;
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly int _xOffset;
        private readonly int _yOffset;

        public InputSystem(World world, int upscaleFactor, int tileWidth, int tileHeight, int mapWidth, int mapHeight, int xOffset, int yOffset) : base(world)
        {
            _upscaleFactor = upscaleFactor;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _xOffset = xOffset;
            _yOffset = yOffset;
        }

        public override void Update(TimeSpan delta)
        {
            var mouseCurrent = Mouse.GetState();
            if (mouseCurrent.LeftButton == ButtonState.Released && mousePrevious.LeftButton == ButtonState.Pressed)
            {
                OnRelease(mouseCurrent.X, mouseCurrent.Y);
            }
            mousePrevious = mouseCurrent;
        }

        private void OnRelease(int mouseX, int mouseY){
            var gridCoords = ScreenToGridCoords(mouseX/_upscaleFactor, mouseY/_upscaleFactor);
        }

        private Vector2 ScreenToGridCoords(int mouseX, int mouseY)
        {
            float mouseFloatX = mouseX - (_tileWidth/2) - _xOffset;
            float mouseFloatY = mouseY - _yOffset;
            float tileWidthFloat = _tileWidth;
            float tileHeightFloat = _tileHeight;

            var gridX = mouseFloatY / tileHeightFloat + mouseFloatX / _tileWidth;
            var gridY = mouseFloatY / tileHeightFloat - mouseFloatX / _tileWidth;
            
            return new Vector2((int)Math.Floor(gridX), (int)Math.Floor(gridY));
        }
    }
}