using System;
using MoonTools.ECS;
using Microsoft.Xna.Framework.Input;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.TempComponents;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Utils;

namespace Enamel.Systems;

public class InputSystem : MoonTools.ECS.System
{
    private Filter SelectableGridCoordFilter { get; }
    private Filter ClickableUiFilter { get; }
    private Filter DraggableFilter { get; }
    private Filter BeingDraggedFilter { get; }
    
    private MouseState _mousePrevious;
    private readonly ScreenUtils _screenUtils;
    
    // Input stuff also happens in ToggleFrameSystem (visual only)
    public InputSystem(World world, ScreenUtils screenUtils) : base(world)
    {
        SelectableGridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        ClickableUiFilter = FilterBuilder
            .Include<OnClickComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        DraggableFilter = FilterBuilder
            .Include<DraggableComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        BeingDraggedFilter = FilterBuilder
            .Include<BeingDraggedFlag>()
            .Build();
        _screenUtils = screenUtils;
    }

    public override void Update(TimeSpan delta)
    {
        var mouseCurrent = Mouse.GetState();
        
        if (mouseCurrent.LeftButton == ButtonState.Pressed && _mousePrevious.LeftButton == ButtonState.Released)
        {
            OnLeftButtonPress();
        }
        if (mouseCurrent.LeftButton == ButtonState.Released && _mousePrevious.LeftButton == ButtonState.Pressed)
        {
            OnLeftButtonRelease();
        }
        if (mouseCurrent.RightButton == ButtonState.Released && _mousePrevious.RightButton == ButtonState.Pressed)
        {
            Send(new CancelMessage());
        }
        _mousePrevious = mouseCurrent;
    }

    private void OnLeftButtonPress()
    {
        foreach (var entity in DraggableFilter.Entities)
        {
            if (_screenUtils.MouseOverEntity(entity))
            {
                Set(entity, new BeingDraggedFlag());
            }
        }
    }

    private void OnLeftButtonRelease(){
        // Release dragged entities
        foreach (var entity in BeingDraggedFilter.Entities)
        {
            Remove<BeingDraggedFlag>(entity);
            Set(entity, new DroppedComponent());
        }
        
        // Button clicks
        foreach (var button in ClickableUiFilter.Entities)
        {
            if (_screenUtils.MouseOverEntity(button))
            {
                // May be better to have "buttonClickMessage" and a dedicated ButtonSystem, but for now this works
                var onClick = Get<OnClickComponent>(button);
                Entity selectedEntity;
                switch (onClick.ClickEvent)
                {
                    case ClickEvent.EndTurn:
                        Send(new EndTurnMessage());
                        break;
                    case ClickEvent.LearnSpell:
                        // Assume the currently selected entity is learning this spell
                        selectedEntity = GetSingletonEntity<SelectedFlag>();
                        // We must assume that the Id is a SpellId if passed along with the LearnSpell ClickEvent
                        Set(selectedEntity, new LearningSpellComponent((SpellId)onClick.Id));
                        break;
                    case ClickEvent.PrepSpell:
                        // Allow for "casterless spells" (just deploying wizards atm)
                        var originX = 0;
                        var originY = 0;
                        if (Some<SelectedFlag>())
                        {
                            // There must only be one selected unit and it must the the unit casting this spell
                            selectedEntity = GetSingletonEntity<SelectedFlag>();
                            if(!Has<GridCoordComponent>(selectedEntity)) continue;
                            var casterCoords = Get<GridCoordComponent>(selectedEntity);
                            originX = casterCoords.X;
                            originY = casterCoords.Y;
                        }

                        // Again we must assume that the Id is a SpellId if passed along with the PrepSpell ClickEvent
                        Send(new PrepSpellMessage((SpellId)onClick.Id, originX, originY));

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
                    case ClickEvent.PreviousCharacter:
                        Send(new PreviousCharacterMessage((Player)onClick.Id));
                        break;
                    case ClickEvent.NextCharacter:
                        Send(new NextCharacterMessage((Player)onClick.Id));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // Unit clicks
        var clickedCoords = _screenUtils.MouseToGridCoords();
        foreach (var entity in SelectableGridCoordFilter.Entities)
        {
            var (x, y) = Get<GridCoordComponent>(entity);
            if((int)clickedCoords.X == x && (int)clickedCoords.Y == y)
            {
                Send(new GridCoordSelectedMessage(x, y));
            }
        }
    }
}