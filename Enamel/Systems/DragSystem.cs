using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Components.TempComponents;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DragSystem : MoonTools.ECS.System
{
    private readonly ScreenUtils _screenUtils;
    private Entity? _dimmer;
    private List<Entity> _litSockets;

    private Filter StartDragFilter { get; }
    private Filter BeingDraggedFilter { get; }
    private Filter EndDragFilter { get; }
    private Filter SocketFilter { get; }

    public DragSystem(World world, ScreenUtils screenUtils) : base(world)
    {
        _screenUtils = screenUtils;
        _dimmer = null;
        _litSockets = new List<Entity>();

        StartDragFilter = FilterBuilder.Include<StartDragComponent>().Build();
        BeingDraggedFilter = FilterBuilder.Include<BeingDraggedFlag>().Build();
        EndDragFilter = FilterBuilder.Include<EndDragComponent>().Build();
        SocketFilter = FilterBuilder.Include<SocketComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in StartDragFilter.Entities)
        {
            Set(entity, new BeingDraggedFlag());
            Remove<StartDragComponent>(entity);
            UnrelateAll<SocketedRelation>(entity);
            CreateDimmer();
            var orbType = Get<OrbTypeComponent>(entity).OrbType;
            HighlightSockets(orbType);
        }

        foreach (var entity in BeingDraggedFilter.Entities)
        {
            var (mouseX, mouseY) = _screenUtils.GetMouseCoords();
            var dimensions = Get<DimensionsComponent>(entity);
            mouseX -= dimensions.Width / 2;
            mouseY -= dimensions.Height / 2;
            Set(entity, new ScreenPositionComponent(mouseX, mouseY));
        }
        
        foreach (var entity in EndDragFilter.Entities)
        {
            Remove<BeingDraggedFlag>(entity);
            Remove<EndDragComponent>(entity);
            DestroyDimmer();
            RemoveSocketHighlights();
            
            var socket = GetSocketUnderMouse();
            var orbType = Get<OrbTypeComponent>(entity).OrbType;
            if (socket != null)
            {
                var socketAcceptsOrbType = Get<SocketComponent>((Entity) socket).ExpectedOrbType.HasFlag(orbType);
                if (socketAcceptsOrbType)
                {
                    var socketCoords = Get<ScreenPositionComponent>((Entity) socket);
                    Set(entity, socketCoords);
                    Relate((Entity)socket, entity, new SocketedRelation());
                    return;
                }
            }
            
            var draggableComponent = Get<DraggableComponent>(entity);
            Set(entity, new MovingToScreenPositionComponent(draggableComponent.OriginalX, draggableComponent.OriginalY, 1000));
        }
    }

    private void HighlightSockets(OrbType orbType)
    {
        foreach (var socket in SocketFilter.Entities)
        {
            var expectedOrbType = Get<SocketComponent>(socket).ExpectedOrbType;
            if (expectedOrbType.HasFlag(orbType))
            {
                Set(socket, new DrawLayerComponent(DrawLayer.Lit));
                Set(socket, new ToggleFrameOnMouseHoverComponent(1));
                _litSockets.Add(socket);
            }
        }
    }

    private void RemoveSocketHighlights()
    {
        foreach (var socket in _litSockets)
        {
            Set(socket, new DrawLayerComponent(DrawLayer.UserInterface));
            Remove<ToggleFrameOnMouseHoverComponent>(socket);
        }
        _litSockets.Clear();
    }

    private void DestroyDimmer()
    {
        if (_dimmer == null) return;
        Destroy((Entity) _dimmer);
        _dimmer = null;
    }

    private void CreateDimmer()
    {
        var dimmer = CreateEntity();
        Set(dimmer, new TextureIndexComponent(Sprite.Dimmer));
        Set(dimmer, new DrawLayerComponent(DrawLayer.Dimmer));
        Set(dimmer, new ScreenPositionComponent(0, 0));
        _dimmer = dimmer;
    }

    private Entity? GetSocketUnderMouse()
    {
        foreach (var entity in SocketFilter.Entities)
        {
            if (_screenUtils.MouseOverEntity(entity))
            {
                return entity;
            }
        }

        return null;
    }
}