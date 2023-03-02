using System;
using System.Numerics;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SelectionPreviewSystem : SpellSystem
{
    private Filter PreviewFilter { get; }
    private Filter ImpassableGridCoordFilter { get; }
    private World _world;

    public SelectionPreviewSystem(World world) : base(world)
    {
        PreviewFilter = FilterBuilder.Include<MovementPreviewFlag>().Build();
        ImpassableGridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Include<ImpassableFlag>().Build();
        _world = world;
    }

    public override void Update(TimeSpan delta)
    {
        var selectMessages = ReadMessages<SelectMessage>();

        // Clear existing previews when a selection is made or turn is ended
        if (!selectMessages.IsEmpty || SomeMessage<EndTurnMessage>())
        {
            foreach (var preview in PreviewFilter.Entities)
            {
                Destroy(preview);
            }
        }

        foreach (var message in selectMessages)
        {
            var entity = message.Entity;
            if (Has<MovesPerTurnComponent>(entity))
            {
                var (x, y) = Get<GridCoordComponent>(entity);
                var pos = new Vector2(x, y);
                CreatePreviewEntities(pos, 1, false, e => Set(e, new MovementPreviewFlag()));
            }
        }

        if (SomeMessage<PrepSpellMessage>())
        {
            var spellToPrepMessage = ReadMessage<PrepSpellMessage>();
            var origin = new Vector2(spellToPrepMessage.OriginGridX, spellToPrepMessage.OriginGridY);
            var spellToPrep = GetSpell(spellToPrepMessage.SpellId);
            var spellRange = Get<CastRangeComponent>(spellToPrep).Range;
            CreatePreviewEntities(origin, spellRange, true, e => Set(e, new SpellPreviewFlag()));
        }
    }

    private void CreatePreviewEntities(Vector2 origin, int range, bool previewOnImpassableTiles, Action<Entity>? applyCustomComponents = null)
    {
        for (var x = 0; x < Constants.MAP_WIDTH; x++)
        {
            for (var y = 0; y < Constants.MAP_HEIGHT; y++)
            {
                if (!previewOnImpassableTiles && ImpassableUnitAtCoord(x, y)) continue;
                var distance = Math.Abs(x - origin.X) + Math.Abs(y - origin.Y);
                if (distance <= range)
                {
                    var preview = CreatePreview(x, y);
                    applyCustomComponents?.Invoke(preview);
                }
            }
        }
    }

    private bool ImpassableUnitAtCoord(int x, int y)
    {
        foreach (var entity in ImpassableGridCoordFilter.Entities)
        {
            var (entityX, entityY) = Get<GridCoordComponent>(entity);
            if (entityX == x && entityY == y)
            {
                return true;
            }
        }

        return false;
    }

    private Entity CreatePreview(int x, int y)
    {
        var preview = _world.CreateEntity();
        Set(preview, new GridCoordComponent(x, y));
        Set(preview, new TextureIndexComponent((int) Sprite.SelectPreview));

        return preview;
    }
}