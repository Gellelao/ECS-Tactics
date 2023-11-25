using System;
using System.Numerics;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SelectionPreviewSystem : SpellSystem
{
    private Filter SpellPreviewFilter { get; }
    private Filter ImpassableGridCoordFilter { get; }
    private readonly World _world;

    public SelectionPreviewSystem(World world) : base(world)
    {
        SpellPreviewFilter = FilterBuilder.Include<SpellToCastOnClickComponent>().Build();
        ImpassableGridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Include<ImpassableFlag>().Build();
        _world = world;
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<CancelMessage>() || SomeMessage<SpellWasCastMessage>() || SomeMessage<EndTurnMessage>())
        {
            DestroySpellPreviews();
        }

        if (SomeMessage<PrepSpellMessage>())
        {
            CreateSpellPreviews();
        }
    }

    private void CreateSpellPreviews()
    {
        // Clear existing previews first
        DestroySpellPreviews();

        var spellToPrepMessage = ReadMessage<PrepSpellMessage>();
        var origin = new Vector2(spellToPrepMessage.OriginGridX, spellToPrepMessage.OriginGridY);
        var spellToPrep = GetSpell(spellToPrepMessage.SpellId);
        var spellRange = Get<CastRangeComponent>(spellToPrep).Range;
        var canTargetImpassable = Has<CanTargetImpassableFlag>(spellToPrep);
        var cardinalOnly = Has<CardinalCastRestrictionFlag>(spellToPrep);
        
        for (var x = 0; x < Constants.MAP_WIDTH; x++)
        {
            for (var y = 0; y < Constants.MAP_HEIGHT; y++)
            {
                if (!canTargetImpassable && ImpassableUnitAtCoord(x, y)) continue;
                if (cardinalOnly && NotCardinal(origin, x, y)) continue;
                var distance = Math.Abs(x - origin.X) + Math.Abs(y - origin.Y);
                if (distance <= spellRange)
                {
                    CreatePreview(x, y, spellToPrepMessage.SpellId);
                }
            }
        }
    }

    private void DestroySpellPreviews()
    {
        foreach (var preview in SpellPreviewFilter.Entities)
        {
            Destroy(preview);
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

    private bool NotCardinal(Vector2 origin, int x, int y)
    {
        return x != (int)origin.X && y != (int)origin.Y;
    }

    private void CreatePreview(int x, int y, SpellId spellId)
    {
        var preview = _world.CreateEntity();
        Set(preview, new GridCoordComponent(x, y));
        Set(preview, new TextureIndexComponent(Sprite.SelectPreview));
        Set(preview, new DrawLayerComponent(DrawLayer.Signage));
        Set(preview, new SpellToCastOnClickComponent(spellId));
    }
}