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
}