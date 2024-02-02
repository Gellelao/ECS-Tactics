using System;
using System.Numerics;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SelectionPreviewSystem : MoonTools.ECS.System
{
    private Filter SpellPreviewFilter { get; }
    private Filter ImpassableGridCoordFilter { get; }
    private readonly World _world;
    private readonly SpellUtils _spellUtils;

    public SelectionPreviewSystem(World world, SpellUtils spellUtils) : base(world)
    {
        SpellPreviewFilter = FilterBuilder.Include<SpellToCastOnSelectComponent>().Build();
        ImpassableGridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Include<ImpassableFlag>().Build();
        _world = world;
        _spellUtils = spellUtils;
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
        var originX = spellToPrepMessage.OriginGridX;
        var originY = spellToPrepMessage.OriginGridY;
        var spellToPrep = _spellUtils.GetSpell(spellToPrepMessage.SpellId);
        var spellRange = Get<CastRangeComponent>(spellToPrep).Range;
        var canTargetImpassable = Has<CanTargetImpassableFlag>(spellToPrep);
        var canTargetSelf = Has<CanTargetSelfFlag>(spellToPrep);
        var cardinalOnly = Has<CardinalCastRestrictionFlag>(spellToPrep);
        
        for (var x = 0; x < Constants.MAP_WIDTH; x++)
        {
            for (var y = 0; y < Constants.MAP_HEIGHT; y++)
            {
                if (!canTargetSelf && TargetingOrigin(originX, originY, x, y)) continue;
                if (!canTargetImpassable && ImpassableUnitAtCoord(x, y)) continue;
                if (cardinalOnly && NotCardinal(originX, originY, x, y)) continue;
                var distance = Math.Abs(x - originX) + Math.Abs(y - originY);
                if (distance <= spellRange)
                {
                    CreateSpellPreview(x, y, spellToPrepMessage.SpellId);
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

    private static bool TargetingOrigin(int originX, int originY, int x, int y)
    {
        return originX == x && originY == y;
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

    private static bool NotCardinal(int originX, int originY, int x, int y)
    {
        return x != originX && y != originY;
    }

    private void CreateSpellPreview(int x, int y, SpellId spellId)
    {
        var preview = _world.CreateEntity();
        Set(preview, new GridCoordComponent(x, y));
        Set(preview, new TextureIndexComponent(Sprite.TileSelectPreview));
        Set(preview, new DrawLayerComponent(DrawLayer.Signage));
        Set(preview, new SpellToCastOnSelectComponent(spellId));
    }
}