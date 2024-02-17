using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Components.TempComponents;
using Enamel.Enums;
using Enamel.Exceptions;
using MoonTools.ECS;

namespace Enamel.Utils;

public class SpellUtils : Manipulator
{
    private Filter SpellIdFilter { get; }

    public SpellUtils(World world) : base(world)
    {
        SpellIdFilter = FilterBuilder.Include<SpellIdComponent>().Build();
    }

    public Entity GetSpell(SpellId spellId)
    {
        foreach (var spell in SpellIdFilter.Entities)
        {
            var id = Get<SpellIdComponent>(spell).SpellId;
            if (id == spellId) return spell;
        }

        throw new SpellNotFoundException($"Did not find spell with id {spellId}");
    }

    public void TeachSpellToEntity(Entity entity, SpellId spell)
    {
        var spellEntity = GetSpell(spell);
        Relate(entity, spellEntity, new HasSpellRelation());
    }
}