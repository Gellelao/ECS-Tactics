using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Enums;
using Enamel.Exceptions;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SpellManagementSystem : MoonTools.ECS.System
{
    private Filter SpellIdFilter { get; }

    public SpellManagementSystem(World world) : base(world)
    {
        SpellIdFilter = FilterBuilder.Include<SpellIdComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<LearnSpellMessage>()) return;

        var spellId = ReadMessage<LearnSpellMessage>().SpellId;
        var spellEntity = GetSpell(spellId);

        try
        {
            var currentlySelectedPlayer = GetSingletonEntity<SelectedFlag>();

            Relate(currentlySelectedPlayer, spellEntity, new HasSpellRelation());
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("No selected player to learn spell!!!");
        }
    }

    private Entity GetSpell(SpellId spellId)
    {
        foreach (var spell in SpellIdFilter.Entities)
        {
            var id = Get<SpellIdComponent>(spell).SpellId;
            if (id == spellId) return spell;
        }

        throw new SpellNotFoundException($"Did not find spell with id {spellId}");
    }
}