using System;
using System.Collections.Generic;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Components.TempComponents;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Extensions;
using Enamel.Utils;

namespace Enamel.Systems;

public class TomesSystem : MoonTools.ECS.System
{
    private readonly World _world;
    private Filter DisplayTomesFilter { get; }

    private readonly List<Entity> _tomes;

    public TomesSystem(World world) : base(world)
    {
        _world = world;
        DisplayTomesFilter = FilterBuilder
            .Include<DisplayTomesComponent>()
            .Build();
        _tomes = new List<Entity>();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in DisplayTomesFilter.Entities)
        {
            // The outer loop only runs when we want to display new cards, so this is where we should destroy old ones
            foreach (var tome in _tomes)
            {
                Destroy(tome);
            }

            _tomes.Clear();

            CreateTomesForEntity(entity);
            Remove<DisplayTomesComponent>(entity);
        }
    }

    private void CreateTomesForEntity(Entity entity)
    {
        var screenX = 0;
        var spells = OutRelations<HasSpellRelation>(entity);
        foreach (var spell in spells)
        {
            var spellIdComponent = Get<SpellIdComponent>(spell);

            var tome = _world.CreateEntity();
            Set(tome, new ScreenPositionComponent(screenX, 0));
            Set(tome, new DimensionsComponent(30, 30));
            Set(tome, new TextureIndexComponent(Sprite.YellowSquare));
            Set(tome, new DrawLayerComponent(DrawLayer.UserInterface));
            var textIndex = TextStorage.GetId(spellIdComponent.SpellId.ToName());
            Set(tome, new TextComponent(textIndex, Font.Absolute, Constants.TomeTextColour));
            Set(tome, new OnClickComponent(ClickEvent.PrepSpell, (int) spellIdComponent.SpellId));
            _tomes.Add(tome);

            screenX += 40;
        }
    }
}