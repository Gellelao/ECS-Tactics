using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.TempComponents;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SpellManagementSystem : MoonTools.ECS.System
{
    private readonly SpellUtils _spellUtils;
    private Filter LearningSpellFilter { get; }

    public SpellManagementSystem(World world, SpellUtils spellUtils) : base(world)
    {
        _spellUtils = spellUtils;
        LearningSpellFilter = FilterBuilder.Include<LearningSpellComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in LearningSpellFilter.Entities)
        {
            var spell = Get<LearningSpellComponent>(entity);
            var spellEntity = _spellUtils.GetSpell(spell.SpellId);
            Relate(entity, spellEntity, new HasSpellRelation());
            Remove<LearningSpellComponent>(entity);
        }
    }
}