using System;
using MoonTools.ECS;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Exceptions;

namespace Enamel.Systems;

public class InputSystem : MoonTools.ECS.System
{
    private Filter SelectableGridCoordFilter { get; }
    private Filter ClickableUIFilter { get; }
    private MouseState _mousePrevious;
    private readonly int _upscaleFactor;
    private readonly int _tileWidth;
    private readonly int _tileHeight;
    private readonly int _xOffset;
    private readonly int _yOffset;

    public InputSystem(World world, int upscaleFactor, int tileWidth, int tileHeight, int xOffset, int yOffset) : base(world)
    {
        SelectableGridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        ClickableUIFilter = FilterBuilder
            .Include<OnClickComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        _upscaleFactor = upscaleFactor;
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
        _xOffset = xOffset;
        _yOffset = yOffset;
    }

    public override void Update(TimeSpan delta)
    {
        var mouseCurrent = Mouse.GetState();

        var mouseX = mouseCurrent.X / _upscaleFactor;
        var mouseY = mouseCurrent.Y / _upscaleFactor;
        OnUpdate(mouseX, mouseY);
        if (mouseCurrent.LeftButton == ButtonState.Released && _mousePrevious.LeftButton == ButtonState.Pressed)
        {
            OnLeftButtonRelease(mouseX, mouseY);
        }
        _mousePrevious = mouseCurrent;
    }

    private void OnUpdate(int mouseX, int mouseY)
    {
        // Button hovers
        foreach (var button in ClickableUIFilter.Entities)
        {
            var dimensions = Get<DimensionsComponent>(button);
            var position = Get<PositionComponent>(button);
            if (MouseInRectangle(mouseX, mouseY, position.X, position.Y, dimensions.Width, dimensions.Height))
            {
                Send(new HighlightMessage(button));
            }
        }

        // Unit hovers
        var gridCoords = ScreenToGridCoords(mouseX, mouseY);
        foreach (var entity in SelectableGridCoordFilter.Entities)
        {
            var (x, y) = Get<GridCoordComponent>(entity);
            if((int)gridCoords.X == x && (int)gridCoords.Y == y)
            {
                Send(new HighlightMessage(entity));
            }
        }
    }

    private void OnLeftButtonRelease(int mouseX, int mouseY){
        // Button clicks
        foreach (var button in ClickableUIFilter.Entities)
        {
            var dimensions = Get<DimensionsComponent>(button);
            var position = Get<PositionComponent>(button);
            if (MouseInRectangle(mouseX, mouseY, position.X, position.Y, dimensions.Width, dimensions.Height))
            {
                // May be better to have "buttonClickMessage" and a dedicated ButtonSystem, but for now this works
                var onClick = Get<OnClickComponent>(button);
                switch (onClick.ClickEvent)
                {
                    case ClickEvent.EndTurn:
                        Send(new EndTurnMessage());
                        break;
                    case ClickEvent.LearnSpell:
                        if (!Has<SpellToLearnOnClickComponent>(button))
                            throw new ComponentNotFoundException(
                                "The LearnSpell click event requires the button entity to also have a SpellToLearnOnClick component");
                        var spellToLearnOnClick = Get<SpellToLearnOnClickComponent>(button).SpellId;

                        Send(new LearnSpellMessage(spellToLearnOnClick));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // Unit clicks
        var clickedCoords = ScreenToGridCoords(mouseX, mouseY);
        foreach (var entity in SelectableGridCoordFilter.Entities)
        {
            var (x, y) = Get<GridCoordComponent>(entity);
            if((int)clickedCoords.X == x && (int)clickedCoords.Y == y)
            {
                Send(new SelectMessage(entity));
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

    private bool MouseInRectangle(int mouseX, int mouseY, int rectX, int rectY, int rectWidth, int rectHeight)
    {
        return mouseX > rectX && mouseX < rectX + rectWidth &&
               mouseY > rectY && mouseY < rectY + rectHeight;
    }
}