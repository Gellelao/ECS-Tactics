using System;
using Enamel.Components;
using Enamel.Enums;
using Enamel.Exceptions;
using MoonTools.ECS;

namespace Enamel.Systems;

// Doesn't seem ideal but I wanted a way to share the GetSpell method across systems and an abstract class
// was the first way I thought of
public abstract class SpellSystem : MoonTools.ECS.System
{
    private Filter SpellIdFilter { get; }

    protected SpellSystem(World world) : base(world)
    {
        SpellIdFilter = FilterBuilder.Include<SpellIdComponent>().Build();
    }

    protected Entity GetSpell(SpellId spellId)
    {
        foreach (var spell in SpellIdFilter.Entities)
        {
            var id = Get<SpellIdComponent>(spell).SpellId;
            Console.WriteLine($"Found spell {id}");
            if (id == spellId) return spell;
        }

        throw new SpellNotFoundException($"Did not find spell with id {spellId}");
    }
}