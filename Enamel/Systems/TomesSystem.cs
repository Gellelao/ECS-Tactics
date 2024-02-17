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
            // The outer loop only runs when we want to display new tomes, so this is where we should destroy old ones
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
        var screenX = Constants.TOME_ROW_STARTING_X;
        var spells = OutRelations<HasSpellRelation>(entity);
        foreach (var spell in spells)
        {
            var spellIdComponent = Get<SpellIdComponent>(spell);
            
            var tome = World.CreateEntity();
            Set(tome, new TextureIndexComponent(Sprite.Tome));
            Set(tome, new ScreenPositionComponent(screenX, 165));
            Set(tome, new DrawLayerComponent(DrawLayer.UserInterface));
            Set(tome, new DimensionsComponent(56, 65));
            var textIndex = TextStorage.GetId(spellIdComponent.SpellId.ToName());
            Set(tome, new TextComponent(textIndex, Font.Absolute, Constants.TomeTextColour));
            Set(tome, new OnClickComponent(ClickEvent.PrepSpell, (int) spellIdComponent.SpellId));

            _tomes.Add(tome);

            screenX += 60;
        }
    }
}