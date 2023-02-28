using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SpellManagementSystem : MoonTools.ECS.System
{
    private Filter ControlledByPlayerFilter { get; }
    private Filter SpellIdFilter { get; }

    public SpellManagementSystem(World world) : base(world)
    {
        ControlledByPlayerFilter = FilterBuilder.Include<ControlledByPlayerComponent>().Build();
        SpellIdFilter = FilterBuilder.Include<SpellIdComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in ControlledByPlayerFilter.Entities)
        {
            if (!SomeMessageWithEntity<LearnSpellMessage>(entity)) continue;

            var spellId = ReadMessageWithEntity<LearnSpellMessage>(entity).SpellId;
            var spellEntity = GetSpell(spellId);
            Relate(entity, spellEntity, new HasSpellRelation());
        }
    }

    private Entity GetSpell(SpellId spellId)
    {
        foreach (var spell in SpellIdFilter.Entities)
        {
            var id = Get<SpellIdComponent>(spell).SpellId;
            if (id == spellId) return spell;
        }

        throw new KeyNotFoundException($"Did not find spell with id {spellId}");
    }
}