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
    private Filter MovementPreviewFilter { get; }
    private Filter SpellPreviewFilter { get; }
    private Filter PlayerControlledFilter { get; }
    private Filter ImpassableGridCoordFilter { get; }
    private World _world;

    public SelectionPreviewSystem(World world) : base(world)
    {
        MovementPreviewFilter = FilterBuilder.Include<MovementPreviewFlag>().Build();
        SpellPreviewFilter = FilterBuilder.Include<SpellPreviewFlag>().Build();
        PlayerControlledFilter = FilterBuilder.Include<ControlledByPlayerComponent>().Build();
        ImpassableGridCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Include<ImpassableFlag>().Build();
        _world = world;
    }

    public override void Update(TimeSpan delta)
    {
        var selectMessages = ReadMessages<SelectMessage>();

        // Clear all existing previews when a selection is made or turn is ended
        if (!selectMessages.IsEmpty || SomeMessage<EndTurnMessage>())
        {
            DestroyMovementPreviews();
            DestroySpellPreviews();
        }

        // Destroy only spell previews when cancel received
        if (SomeMessage<CancelMessage>())
        {
            DestroySpellPreviews();

            // Re-create any movement previews that we destroyed when prepping the spell
            CreateMovementPreviews(GetSingletonEntity<SelectedFlag>());
        }

        foreach (var selectable in PlayerControlledFilter.Entities)
        {
            if (SomeMessageWithEntity<SpellWasCastMessage>(selectable) ||
                SomeMessageWithEntity<UnitMoveCompletedMessage>(selectable) ||
                SomeMessageWithEntity<PlayerUnitSelectedMessage>(selectable))
            {
                CreateMovementPreviews(selectable);
            }
        }

        if (SomeMessage<PrepSpellMessage>())
        {
            // Destroy movement previews so there's no ambiguity on the clicked tile
            DestroyMovementPreviews();

            CreateSpellPreviews();
        }
    }

    private void CreateMovementPreviews(Entity movingUnit)
    {
        if (Has<MovesPerTurnComponent>(movingUnit))
        {
            var remainingMoves = Get<RemainingMovesComponent>(movingUnit).RemainingMoves;
            if (remainingMoves <= 0) return;

            var (x, y) = Get<GridCoordComponent>(movingUnit);
            var pos = new Vector2(x, y);
            CreatePreviewEntities(pos, 1, false, false, e => Set(e, new MovementPreviewFlag()));
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
        CreatePreviewEntities(origin, spellRange, canTargetImpassable, cardinalOnly, e =>
        {
            Set(e, new SpellPreviewFlag());
            Set(e, new SpellToCastOnClickComponent(spellToPrepMessage.SpellId));
        });
    }

    private void DestroyMovementPreviews()
    {

        foreach (var preview in MovementPreviewFilter.Entities)
        {
            Destroy(preview);
        }
    }

    private void DestroySpellPreviews()
    {
        foreach (var preview in SpellPreviewFilter.Entities)
        {
            Destroy(preview);
        }
    }

    private void CreatePreviewEntities(Vector2 origin, int range, bool previewOnImpassableTiles, bool cardinalOnly, Action<Entity>? applyCustomComponents = null)
    {
        for (var x = 0; x < Constants.MAP_WIDTH; x++)
        {
            for (var y = 0; y < Constants.MAP_HEIGHT; y++)
            {
                if (!previewOnImpassableTiles && ImpassableUnitAtCoord(x, y)) continue;
                if(cardinalOnly && NotCardinal(origin, x, y)) continue;
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

    private bool NotCardinal(Vector2 origin, int x, int y)
    {
        return x != (int)origin.X && y != (int)origin.Y;
    }

    private Entity CreatePreview(int x, int y)
    {
        var preview = _world.CreateEntity();
        Set(preview, new GridCoordComponent(x, y));
        Set(preview, new TextureIndexComponent(Sprite.SelectPreview));

        return preview;
    }
}