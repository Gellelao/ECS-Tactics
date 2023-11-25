using System;
using System.Net;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.TempComponents;
using Enamel.Components.UI;

namespace Enamel.Systems;

public class UnitSelectionSystem : MoonTools.ECS.System
{
    private Filter SelectableCoordFilter { get; }
    private Filter SelectedFilter { get; }

    public UnitSelectionSystem(World world) : base(world)
    {
        SelectableCoordFilter = FilterBuilder
            .Include<GridCoordComponent>()
            .Exclude<SpellToCastOnClickComponent>()
            .Exclude<DisabledFlag>()
            .Build();
        SelectedFilter = FilterBuilder.Include<SelectedFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<GridCoordSelectedMessage>()) return;
        var (selectingX, selectingY) = ReadMessage<GridCoordSelectedMessage>();
        foreach (var entity in SelectableCoordFilter.Entities)
        {
            var (entityX, entityY) = Get<GridCoordComponent>(entity);
            if (entityX != selectingX || entityY != selectingY) continue;
            
            foreach (var selectedEntity in SelectedFilter.Entities){
                Remove<SelectedFlag>(selectedEntity);
            }

            Set(entity, new SelectedFlag());
            Set(entity, new DisplaySpellCardsComponent());
        }
    }
}