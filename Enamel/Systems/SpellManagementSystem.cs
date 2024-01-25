using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SpellManagementSystem(World world, SpellUtils spellUtils) : MoonTools.ECS.System(world)
{
    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<LearnSpellMessage>()) return;

        var spells = ReadMessages<LearnSpellMessage>();

        try
        {
            var currentlySelectedPlayer = GetSingletonEntity<SelectedFlag>();
            foreach (var spell in spells)
            {
                var spellEntity = spellUtils.GetSpell(spell.SpellId);
                Relate(currentlySelectedPlayer, spellEntity, new HasSpellRelation());
            }
        }
        catch (IndexOutOfRangeException)
        {
            throw new IndexOutOfRangeException("No selected player to learn spell!!!");
        }
    }
}