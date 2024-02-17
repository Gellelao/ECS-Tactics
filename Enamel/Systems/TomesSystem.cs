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
    private readonly MenuUtils _menuUtils;
    private Filter DisplayTomesFilter { get; }

    private readonly List<Entity> _tomes;

    public TomesSystem(World world, MenuUtils menuUtils) : base(world)
    {
        _menuUtils = menuUtils;
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
                _menuUtils.RecursivelyDestroy(tome);
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
            
            var tome = _menuUtils.CreateUiEntity(screenX, Constants.TOME_ROW_DEFAULT_Y, 56, 65);
            Set(tome, new TextureIndexComponent(Sprite.Tome));
            Set(tome, new OnClickComponent(ClickEvent.PrepSpell, (int) spellIdComponent.SpellId));
            
            var textIndex = TextStorage.GetId(spellIdComponent.SpellId.ToName());
            Set(tome, new TextComponent(textIndex, Font.Absolute, Constants.TomeTextColour));

            CreateSocket(tome, 5, 15);

            _tomes.Add(tome);

            screenX += 60;
        }
    }

    private void CreateSocket(Entity tome, int x, int y)
    {
        var testSocket = _menuUtils.CreateRelativeUiEntity(tome, x, y, 12, 12);
        World.Set(testSocket, new TextureIndexComponent(Sprite.Socket));
        World.Set(testSocket, new SocketComponent());
        World.Set(testSocket, new ToggleFrameOnMouseHoverComponent(1));
        // TODO Some smart system which
        // 1. lets you know which sockets accept the orb you are holding
        // 2. lets you know which socket will take the orb you are holding if you release it now
        World.Set(testSocket, new AnimationSetComponent(AnimationSet.Socket));
    }
}