﻿using System;
using Enamel.Components;
using Enamel.Components.TempComponents;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DragSystem : MoonTools.ECS.System
{
    private readonly ScreenUtils _screenUtils;

    private Filter BeingDraggedFilter { get; }
    private Filter DroppedFilter { get; }
    private Filter SocketFilter { get; }

    public DragSystem(World world, ScreenUtils screenUtils) : base(world)
    {
        _screenUtils = screenUtils;

        BeingDraggedFilter = FilterBuilder.Include<BeingDraggedFlag>().Build();
        DroppedFilter = FilterBuilder.Include<DroppedComponent>().Build();
        SocketFilter = FilterBuilder.Include<SocketComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in DroppedFilter.Entities)
        {
            Remove<DroppedComponent>(entity);
            var socket = GetSocketUnderMouse();
            if (socket != null)
            {
                var socketCoords = Get<ScreenPositionComponent>((Entity) socket);
                Set(entity, socketCoords);
            }
            else
            {
                var draggableComponent = Get<DraggableComponent>(entity);
                Set(entity, new ScreenPositionComponent(draggableComponent.OriginalX, draggableComponent.OriginalY));
            }
        }

        foreach (var entity in BeingDraggedFilter.Entities)
        {
            var socket = GetSocketUnderMouse();
            if (socket != null)
            {
                var socketCoords = Get<ScreenPositionComponent>((Entity) socket);
                Set(entity, socketCoords);
            }
            else
            {
                var (mouseX, mouseY) = _screenUtils.GetMouseCoords();
                Set(entity, new ScreenPositionComponent(mouseX, mouseY));
            }
        }
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