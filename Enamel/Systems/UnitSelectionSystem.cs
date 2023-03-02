using System;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Extensions;

namespace Enamel.Systems;

public class UnitSelectionSystem : MoonTools.ECS.System
{
    private readonly World _world;
    private Filter SelectableCoordFilter { get; }
    private Filter SelectedFilter { get; }
    private Filter SpellCardFilter { get; }

    public UnitSelectionSystem(World world) : base(world)
    {
        _world = world;
        SelectableCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Exclude<MovementPreviewFlag>()
            .Exclude<DisabledFlag>()
            .Build();
        SelectedFilter = FilterBuilder.Include<SelectedFlag>().Build();
        SpellCardFilter = FilterBuilder.Include<SpellToPrepOnClickComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in SelectableCoordFilter.Entities)
        {
            if (!SomeMessageWithEntity<SelectMessage>(entity)) continue;

            foreach (var selectedEntity in SelectedFilter.Entities){
                Remove<SelectedFlag>(selectedEntity);
            }

            foreach (var spellCard in SpellCardFilter.Entities)
            {
                Destroy(spellCard);
            }

            Set(entity, new SelectedFlag());
            CreateSpellCardsForEntity(entity);
        }
    }

    private void CreateSpellCardsForEntity(Entity entity)
    {
        var screenX = 0;
        var (gridOriginX, gridOriginY) = Get<GridCoordComponent>(entity);
        foreach (var (spell, _) in OutRelations<HasSpellRelation>(entity))
        {
            var spellIdComponent = Get<SpellIdComponent>(spell);
            Console.WriteLine($"Selected players knows spell {spellIdComponent}");

            var spellCard = _world.CreateEntity();
            Set(spellCard, new PositionComponent(screenX, 320));
            Set(spellCard, new DimensionsComponent(30, 30));
            Set(spellCard, new TextureIndexComponent((int) Sprite.YellowSquare));
            Set(spellCard, new TextComponent(TextStorage.GetId(spellIdComponent.SpellId.ToName()), Constants.SPELL_CARD_TEXT_SIZE, Constants.SpellCardTextColour));
            Set(spellCard, new OnClickComponent(ClickEvent.PrepSpell));
            Set(spellCard, new SpellToPrepOnClickComponent(spellIdComponent.SpellId, gridOriginX, gridOriginY));
            screenX += 40;
        }
    }
}