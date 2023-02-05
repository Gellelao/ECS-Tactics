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
        var selectMessages = ReadMessages<Select>();

        // Clear existing previews when a selection is made
        if (!selectMessages.IsEmpty)
        {
            _previewEntities.ForEach(e => Destroy(e));
        }

        foreach (var message in selectMessages)
        {
            var entity = message.Entity;
            if (Has<MovementRangeComponent>(entity))
            {
                var gridCoordComponent = Get<GridCoordComponent>(entity);
                var pos = new Vector2(gridCoordComponent.X, gridCoordComponent.Y);
                CreatePreviewEntities(pos, Get<MovementRangeComponent>(entity).Range);
            }
        }
    }

    private void CreatePreviewEntities(Vector2 origin, int range)
    {
        var diameter = 1 + 2 * range;
        for (var x = 0; x < diameter; x++)
        {
            for (var y = 0; y < diameter; y++)
            {
                CreatePreview(x, y);
            }
        }
    }

    private void CreatePreview(int x, int y)
    {
        var preview = _world.CreateEntity();
        Set(preview, new GridCoordComponent(x, y));
        Set(preview, new TextureIndexComponent((int) Sprite.SelectPreview));
        Set(preview, new SpriteOriginComponent(0, 1));
        _previewEntities.Add(preview);
    }
}