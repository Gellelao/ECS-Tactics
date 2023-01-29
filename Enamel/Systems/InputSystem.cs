using System;
using MoonTools.ECS;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Enamel.Systems
{
    public class InputSystem : MoonTools.ECS.System
    {
	    private MouseState mousePrevious = new MouseState();
        private int _tileWidth;
        private int _tileHeight;
        private int _mapWidth;
        private int _mapHeight;

        public InputSystem(World world, int tileWidth, int tileHeight, int mapWidth, int mapHeight) : base(world)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
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
            Console.WriteLine($"mouseX: {mouseX}, mouseY: {mouseY}");
            // Divide by 2 since we upscale the screen 2x
            var gridCoords = ScreenToGridCoords(mouseX/2, mouseY/2);
            Console.WriteLine($"X: {gridCoords.X}, Y: {gridCoords.Y}");
        }

        private Vector2 ScreenToGridCoords(int mouseX, int mouseY)
        {
            float mouseFloatX = mouseX;
            float mouseFloatY = mouseY;
            float tileWidthFloat = _tileWidth;
            float tileHeightFloat = _tileHeight;

            var gridX = mouseFloatX / _tileWidth + mouseFloatY / tileHeightFloat;
            var gridY = mouseFloatY / tileHeightFloat - mouseFloatX / _tileWidth;
            return new Vector2((int)Math.Floor(gridX)-4, (int)Math.Floor(gridY)+4);
        }
    }
}