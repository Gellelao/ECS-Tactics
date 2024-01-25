using System;
using Enamel.Components.Messages;
using Microsoft.Xna.Framework;
using MoonTools.ECS;

namespace Enamel.Utils;

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

    public Vector2 ScreenToGridCoords(int mouseX, int mouseY)
    {
        // Camera pos is determined by the game, while the screen offsets are set if the user resizes the window to non-16:9 resolutions 
        float mouseFloatX = mouseX - (Constants.TILE_WIDTH/2) - cameraX - ScreenOffsetX;
        float mouseFloatY = mouseY - cameraY - ScreenOffsetY;
        const float tileWidthFloat = Constants.TILE_WIDTH;
        const float tileHeightFloat = Constants.TILE_HEIGHT;

        var gridX = mouseFloatY / tileHeightFloat + mouseFloatX / tileWidthFloat;
        var gridY = mouseFloatY / tileHeightFloat - mouseFloatX / tileWidthFloat;
            
        return new Vector2((int)Math.Floor(gridX), (int)Math.Floor(gridY));
    }

    public bool MouseInRectangle(int mouseX, int mouseY, int rectX, int rectY, int rectWidth, int rectHeight)
    {
        mouseX -= ScreenOffsetX;
        mouseY -= ScreenOffsetY;
        return mouseX > rectX && mouseX < rectX + rectWidth &&
               mouseY > rectY && mouseY < rectY + rectHeight;
    }
}