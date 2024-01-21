using System;
using System.Collections.Generic;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.TempComponents;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Extensions;

namespace Enamel.Systems;

public class PlayerButtonsSystem : MoonTools.ECS.System
{
    private readonly World _world;
    private Filter DisplaySpellCardsFilter { get; }

    private readonly List<Entity> _spellCards;

    public PlayerButtonsSystem(World world) : base(world)
    {
        _world = world;
        DisplaySpellCardsFilter = FilterBuilder
            .Include<DisplaySpellCardsComponent>()
            .Build();
        _spellCards = new List<Entity>();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in DisplaySpellCardsFilter.Entities)
        {
            // The outer loop only runs when we want to display new cards, so this is where we should destroy old ones
            foreach (var spellCard in _spellCards)
            {
                Destroy(spellCard);
            }
            _spellCards.Clear();

            CreateSpellCardsForEntity(entity);
            Remove<DisplaySpellCardsComponent>(entity);
        }
    }

    private void CreateSpellCardsForEntity(Entity entity)
    {
        var screenX = 0;
        var spells = OutRelations<HasSpellRelation>(entity);
        foreach (var spell in spells)
        {
            var spellIdComponent = Get<SpellIdComponent>(spell);

            var spellCard = _world.CreateEntity();
            Set(spellCard, new ScreenPositionComponent(screenX, 0));
            Set(spellCard, new DimensionsComponent(30, 30));
            Set(spellCard, new TextureIndexComponent(Sprite.YellowSquare));
            Set(spellCard, new DrawLayerComponent(DrawLayer.UserInterface));
            Set(spellCard, new TextComponent(TextStorage.GetId(spellIdComponent.SpellId.ToName()), Font.Absolute, Constants.SpellCardTextColour));
            Set(spellCard, new OnClickComponent(ClickEvent.PrepSpell, (int)spellIdComponent.SpellId));
            _spellCards.Add(spellCard);
            
            screenX += 40;
        }
    }
}