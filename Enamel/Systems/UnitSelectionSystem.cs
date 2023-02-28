using System;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;

namespace Enamel.Systems;

public class UnitSelectionSystem : MoonTools.ECS.System
{
    private Filter SelectableCoordFilter { get; }
    private Filter SelectedFilter { get; }

    public UnitSelectionSystem(World world) : base(world)
    {
        SelectableCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Exclude<MovementPreviewFlag>()
            .Exclude<DisabledFlag>()
            .Build();
        SelectedFilter = FilterBuilder.Include<SelectedFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in SelectableCoordFilter.Entities)
        {
            if (!SomeMessageWithEntity<SelectMessage>(entity)) continue;

            foreach (var selectedEntity in SelectedFilter.Entities){
                Remove<SelectedFlag>(selectedEntity);
            }
            Set(entity, new SelectedFlag());
            CreateSpellCardsForEntity(entity);
        }
    }

    private void CreateSpellCardsForEntity(Entity entity)
    {
        foreach (var (spell, _) in OutRelations<HasSpellRelation>(entity))
        {
            var spellId = Get<SpellIdComponent>(spell);
            Console.WriteLine($"Selected players knows spell {spellId}");
        }
    }
}