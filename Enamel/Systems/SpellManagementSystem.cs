using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SpellManagementSystem : SpellSystem
{
    public SpellManagementSystem(World world) : base(world) { }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<LearnSpellMessage>()) return;

        var spells = ReadMessages<LearnSpellMessage>();

        try
        {
            var currentlySelectedPlayer = GetSingletonEntity<SelectedFlag>();
            foreach (var spell in spells)
            {
                var spellEntity = GetSpell(spell.SpellId);
                Relate(currentlySelectedPlayer, spellEntity, new HasSpellRelation());
            }
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("No selected player to learn spell!!!");
        }
    }
}