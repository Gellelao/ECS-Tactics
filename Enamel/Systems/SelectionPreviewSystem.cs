using System;
using System.Collections.Generic;
using System.Numerics;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SelectionPreviewSystem : MoonTools.ECS.System
{
    private List<Entity> _previewEntities;
    private World _world;

    public SelectionPreviewSystem(World world) : base(world)
    {
        _world = world;
        _previewEntities = new List<Entity>();
    }

    public override void Update(TimeSpan delta)
    {
        var selectMessages = ReadMessages<SelectMessage>();

        // Clear existing previews when a selection is made
        if (!selectMessages.IsEmpty)
        {
            _previewEntities.ForEach(e => Destroy(e));
            _previewEntities = new List<Entity>();
        }

        foreach (var message in selectMessages)
        {
            var entity = message.Entity;
            if (Has<MovesPerTurnComponent>(entity))
            {
                var gridCoordComponent = Get<GridCoordComponent>(entity);
                var pos = new Vector2(gridCoordComponent.X, gridCoordComponent.Y);
                CreatePreviewEntities(pos, 1);
            }
        }
    }

    private void CreatePreviewEntities(Vector2 origin, int range)
    {
        for (var x = 0; x < Constants.MapWidth; x++)
        {
            for (var y = 0; y < Constants.MapHeight; y++)
            {
                var distance = Math.Abs(x - origin.X) + Math.Abs(y - origin.Y);
                if (distance <= range)
                {
                    CreatePreview(x, y);
                }
            }
        }
    }

    private void CreatePreview(int x, int y)
    {
        var preview = _world.CreateEntity();
        Set(preview, new GridCoordComponent(x, y));
        Set(preview, new TextureIndexComponent((int) Sprite.SelectPreview));
        Set(preview, new SelectableFlag());
        Set(preview, new MovementPreviewFlag());
        _previewEntities.Add(preview);
    }
}