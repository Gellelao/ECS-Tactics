using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.UI;
using Enamel.Extensions;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SpellCastingSystem : SpellSystem
{
    private Filter SpellPreviewFilter { get; }

    public SpellCastingSystem(World world) : base(world)
    {
        SpellPreviewFilter = FilterBuilder.Include<SpellPreviewFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var spellPreview in SpellPreviewFilter.Entities)
        {
            if (!SomeMessageWithEntity<SelectMessage>(spellPreview)) continue;

            var targetScreenPosition = Get<PositionComponent>(spellPreview);
            var targetGridPosition = Get<GridCoordComponent>(spellPreview);
            var selectedEntity = GetSingletonEntity<SelectedFlag>();
            var (x, y) = Get<GridCoordComponent>(selectedEntity);

            var spellToCastComponent = Get<SpellToCastOnClickComponent>(spellPreview);
            var spell = GetSpell(spellToCastComponent.SpellId);

            Console.WriteLine($"{spellToCastComponent.SpellId.ToName()} cast at {targetGridPosition.X},{targetGridPosition.Y} by unit at {x},{y}");
        }
    }
}