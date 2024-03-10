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

        foreach(var tome in _tomes){
            if(SocketsSatisfied(tome)){
                Remove<DisabledFlag>(tome);
            }
            else{
                Set(tome, new DisabledFlag());
            }
        }

        if (SomeMessage<SpellWasCastMessage>())
        {
            var justCastSpellId = ReadMessage<SpellWasCastMessage>().SpellId;
            foreach (var tome in _tomes)
            {
                if (Has<DisabledFlag>(tome)) continue;
                var tomeSpellId = (SpellId)Get<OnClickComponent>(tome).Id; // As in the InputSystem, we are assuming this id refers to a spellId...
                if (tomeSpellId == justCastSpellId)
                {
                    DestroyOrbsInTome(tome);
                }
            }
        }
    }

    private void DestroyOrbsInTome(Entity tome)
    {
        var sockets = OutRelations<IsParentRelation>(tome);
        foreach (var socket in sockets)
        {
            if (!HasOutRelation<SocketedRelation>(socket)) continue;
            var socketedEntity = OutRelationSingleton<SocketedRelation>(socket);
            var currentPlayerId = Get<PlayerIdComponent>(GetSingletonEntity<CurrentPlayerFlag>()).PlayerId;
            Set(socketedEntity, new ToBeDiscardedComponent(currentPlayerId));
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

            // This should pretty much always be true, DeployWizards doesn't have requirements but it shouldn't render a tome...
            if (Has<OrbRequirementComponent>(spell))
            {
                var requirements = Get<OrbRequirementComponent>(spell);
                CreateSockets(tome, requirements);
            }

            _tomes.Add(tome);

            screenX += 60;
        }
    }

    private void CreateSockets(Entity tome, OrbRequirementComponent requirements)
    {
        var x = 4;
        var xIncrement = 15;
        var y = 15;
        for (var i = 0; i < requirements.AnyType; i++)
        {
            CreateSocket(tome, true, OrbType.Any, x, y);
            x += xIncrement;
        }
        for (var i = 0; i < requirements.Arcane; i++)
        {
            CreateSocket(tome, true, OrbType.Arcane, x, y);
            x += xIncrement;
        }
    }

    private void CreateSocket(Entity tome, bool required, OrbType expectedOrbType, int x, int y)
    {
        var socket = _menuUtils.CreateRelativeUiEntity(tome, x, y, 12, 12);
        World.Set(socket, new TextureIndexComponent(expectedOrbType.ToSocketSprite()));
        World.Set(socket, new SocketComponent(required, expectedOrbType));
        World.Set(socket, new AnimationSetComponent(AnimationSet.Socket));
    }

    private bool SocketsSatisfied(Entity tome){
        var children = OutRelations<IsParentRelation>(tome);
        foreach(var child in children){
            if(Has<SocketComponent>(child)){
                var socketComponent = Get<SocketComponent>(child);
                if(!socketComponent.Required) continue;
                // If it has an orb in it, it must be satisfied
                if(!HasOutRelation<SocketedRelation>(child)){
                    return false;
                }
            }
        }
        return true;
    }
}