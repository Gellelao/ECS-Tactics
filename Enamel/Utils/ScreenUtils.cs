using System;
using Enamel.Components.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;

namespace Enamel.Utils;

// Idk if this is bad but this system is updated like other systems to receive any screen changes
// and its passed in to other systems so they can get the current screen details from it.
// Feels weird like maybe a code smell to have it like that but works for now
public class ScreenUtils(World world, int cameraX, int cameraY) : MoonTools.ECS.System(world)
{
    public int ScreenOffsetX { get; private set; }
    public int ScreenOffsetY { get; private set; }
    public float Scale { get; private set; }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<ScreenDetailsChangedMessage>()) return;
        var newScreenDetails = ReadMessage<ScreenDetailsChangedMessage>();
        Scale = newScreenDetails.Scale;
        ScreenOffsetX = (int)Math.Round(newScreenDetails.OffsetX/Scale);
        ScreenOffsetY = (int)Math.Round(newScreenDetails.OffsetY/Scale);
    }

    public Vector2 MouseToGridCoords(){
        var (mouseX, mouseY) = GetMouseCoords();
        return ScreenToGridCoords(mouseX, mouseY);
    }

    public bool MouseInRectangle(int rectX, int rectY, int rectWidth, int rectHeight)
    {
        var (mouseX, mouseY) = GetMouseCoords();

        mouseX -= ScreenOffsetX;
        mouseY -= ScreenOffsetY;
        return mouseX > rectX && mouseX < rectX + rectWidth &&
               mouseY > rectY && mouseY < rectY + rectHeight;
    }

    private Vector2 ScreenToGridCoords(int screenX, int screenY)
    {
        // Camera pos is determined by the game, while the screen offsets are set if the user resizes the window to non-16:9 resolutions 
        float mouseFloatX = screenX - (Constants.TILE_WIDTH/2) - cameraX - ScreenOffsetX;
        float mouseFloatY = screenY - cameraY - ScreenOffsetY;
        const float tileWidthFloat = Constants.TILE_WIDTH;
        const float tileHeightFloat = Constants.TILE_HEIGHT;

        var gridX = mouseFloatY / tileHeightFloat + mouseFloatX / tileWidthFloat;
        var gridY = mouseFloatY / tileHeightFloat - mouseFloatX / tileWidthFloat;
            
        return new Vector2((int)Math.Floor(gridX), (int)Math.Floor(gridY));
    }

    public (int mouseX, int mouseY) GetMouseCoords(){
        
        var mouseCurrent = Mouse.GetState();
        
        int mouseX = (int)Math.Round(mouseCurrent.X / Scale);
        int mouseY = (int)Math.Round(mouseCurrent.Y / Scale);

        return (mouseX, mouseY);
    }
}