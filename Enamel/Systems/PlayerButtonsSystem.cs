using System;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Extensions;

namespace Enamel.Systems;

public class PlayerButtonsSystem : MoonTools.ECS.System
{
    private readonly World _world;
    private Filter ControlledByPlayerFilter { get; }
    private Filter SpellCardFilter { get; }

    public PlayerButtonsSystem(World world) : base(world)
    {
        _world = world;
        ControlledByPlayerFilter = FilterBuilder
            .Include<ControlledByPlayerComponent>()
            .Build();
        SpellCardFilter = FilterBuilder.Include<SpellToPrepOnClickComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in ControlledByPlayerFilter.Entities)
        {
            if (!SomeMessageWithEntity<PlayerUnitSelectedMessage>(entity)) continue;

            foreach (var spellCard in SpellCardFilter.Entities)
            {
                Destroy(spellCard);
            }

            CreateSpellCardsForEntity(entity);
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
            Set(spellCard, new PositionComponent(screenX, 320));
            Set(spellCard, new DimensionsComponent(30, 30));
            Set(spellCard, new TextureIndexComponent(Sprite.YellowSquare));
            Set(spellCard, new TextComponent(TextStorage.GetId(spellIdComponent.SpellId.ToName()), Constants.SPELL_CARD_TEXT_SIZE, Constants.SpellCardTextColour));
            Set(spellCard, new OnClickComponent(ClickEvent.PrepSpell));
            Set(spellCard, new SpellToPrepOnClickComponent(spellIdComponent.SpellId));
            screenX += 40;
        }
    }
}