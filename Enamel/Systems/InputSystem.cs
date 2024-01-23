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
    private Filter ClickableUiFilter { get; }
    private Filter HighlightedFilter { get; }
    private MouseState _mousePrevious;
    private readonly int _cameraX;
    private readonly int _cameraY;
    private int _screenOffsetX;
    private int _screenOffsetY;
    private float _scale;

    public InputSystem(World world, float scale, int cameraX, int cameraY) : base(world)
    {
        SelectableGridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        ClickableUiFilter = FilterBuilder
            .Include<OnClickComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        HighlightedFilter = FilterBuilder.Include<HighlightedFlag>().Build();
        _cameraX = cameraX;
        _cameraY = cameraY;
        _screenOffsetX = 0;
        _screenOffsetY = 0;
        _scale = scale;
    }


    public override void Update(TimeSpan delta)
    {
        var mouseCurrent = Mouse.GetState();

        if(SomeMessage<ScreenDetailsChangedMessage>()){
            var newScreenDetails = ReadMessage<ScreenDetailsChangedMessage>();
            _scale = newScreenDetails.Scale;
            _screenOffsetX = (int)Math.Round(newScreenDetails.OffsetX/_scale);
            _screenOffsetY = (int)Math.Round(newScreenDetails.OffsetY/_scale);
        }

        int mouseX = (int)Math.Round(mouseCurrent.X / _scale);
        int mouseY = (int)Math.Round(mouseCurrent.Y / _scale);

        OnUpdate(mouseX, mouseY);
        if (mouseCurrent.LeftButton == ButtonState.Released && _mousePrevious.LeftButton == ButtonState.Pressed)
        {
            OnLeftButtonRelease(mouseX, mouseY);
        }
        if (mouseCurrent.RightButton == ButtonState.Released && _mousePrevious.RightButton == ButtonState.Pressed)
        {
            Send(new CancelMessage());
        }
        _mousePrevious = mouseCurrent;
    }

    private void OnUpdate(int mouseX, int mouseY)
    {
        // Clear existing hovers
        foreach (var selectedEntity in HighlightedFilter.Entities){
            Remove<HighlightedFlag>(selectedEntity);
        }
        
        // Button hovers
        foreach (var button in ClickableUiFilter.Entities)
        {
            var dimensions = Get<DimensionsComponent>(button);
            var position = Get<ScreenPositionComponent>(button);
            if (MouseInRectangle(mouseX, mouseY, position.X, position.Y, dimensions.Width, dimensions.Height))
            {
                Set(button, new HighlightedFlag());
            }
        }

        // Unit hovers
        var gridCoords = ScreenToGridCoords(mouseX, mouseY);
        foreach (var entity in SelectableGridCoordFilter.Entities)
        {
            var (x, y) = Get<GridCoordComponent>(entity);
            if((int)gridCoords.X == x && (int)gridCoords.Y == y)
            {
                Set(entity, new HighlightedFlag());
            }
        }
    }

    private void OnLeftButtonRelease(int mouseX, int mouseY){
        // Button clicks
        foreach (var button in ClickableUiFilter.Entities)
        {
            var dimensions = Get<DimensionsComponent>(button);
            var position = Get<ScreenPositionComponent>(button);
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
                        // We must assume that the Id is a SpellId if passed along with the LearnSpell ClickEvent
                        Send(new LearnSpellMessage((SpellId)onClick.Id));
                        break;
                    case ClickEvent.PrepSpell:
                        // There must only be one selected unit and it must the the unit casting this spell
                        var selectedEntity = GetSingletonEntity<SelectedFlag>();
                        if(!Has<GridCoordComponent>(selectedEntity)) continue;
                        var (originGridX, originGridY) = Get<GridCoordComponent>(selectedEntity);

                        // Again we must assume that the Id is a SpellId if passed along with the PrepSpell ClickEvent
                        Send(new PrepSpellMessage((SpellId)onClick.Id, originGridX, originGridY));

                        break;
                    case ClickEvent.GoToMainMenu:
                        Send(new GoToMainMenuMessage());
                        break;
                    case ClickEvent.GoToCharacterSelect:
                        Send(new GoToCharacterSelectMessage());
                        break;
                    case ClickEvent.OpenOptions:
                        Send(new OpenOptionsMessage());
                        break;
                    case ClickEvent.ExitGame:
                        Send(new ExitGameMessage());
                        break;
                    case ClickEvent.DeployWizards:
                        Send(new DeployWizardsMessage());
                        break;
                    case ClickEvent.AddPlayer:
                        Send(new AddPlayerMessage());
                        break;
                    case ClickEvent.DeletePlayer:
                        Send(new DeletePlayerMessage());
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
                Send(new GridCoordSelectedMessage(x, y));
            }
        }
    }

    private Vector2 ScreenToGridCoords(int mouseX, int mouseY)
    {
        // Camera pos is determined by the game, while the screen offsets are set if the user resizes the window to non-16:9 resolutions 
        float mouseFloatX = mouseX - (Constants.TILE_WIDTH/2) - _cameraX - _screenOffsetX;
        float mouseFloatY = mouseY - _cameraY - _screenOffsetY;
        const float tileWidthFloat = Constants.TILE_WIDTH;
        const float tileHeightFloat = Constants.TILE_HEIGHT;

        var gridX = mouseFloatY / tileHeightFloat + mouseFloatX / tileWidthFloat;
        var gridY = mouseFloatY / tileHeightFloat - mouseFloatX / tileWidthFloat;
            
        return new Vector2((int)Math.Floor(gridX), (int)Math.Floor(gridY));
    }

    private bool MouseInRectangle(int mouseX, int mouseY, int rectX, int rectY, int rectWidth, int rectHeight)
    {
        mouseX -= _screenOffsetX;
        mouseY -= _screenOffsetY;
        return mouseX > rectX && mouseX < rectX + rectWidth &&
               mouseY > rectY && mouseY < rectY + rectHeight;
    }
}