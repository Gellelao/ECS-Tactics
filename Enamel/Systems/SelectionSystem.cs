using System;
using MoonTools.ECS;
using Enamel.Components;
using Microsoft.Xna.Framework;
using Enamel.Components.Messages;

namespace Enamel.Systems
{
    public class SelectionSystem : MoonTools.ECS.System
    {
        private Filter SelectableFilter { get; }
        private Filter SelectedFilter { get; }
        private Filter HighlightedFilter { get; }

        public SelectionSystem(World world) : base(world)
        {
            SelectableFilter = FilterBuilder
                .Include<SelectableComponent>()
                .Build();
            SelectedFilter = FilterBuilder
                .Include<SelectedComponent>()
                .Build();
            HighlightedFilter = FilterBuilder
                .Include<HighlightedComponent>()
                .Build();
        }

        public override void Update(TimeSpan delta)
        {
            foreach (var entity in SelectableFilter.Entities)
            {
                if (SomeMessageWithEntity<Highlight>(entity))
                {
                    foreach (var selectedEntity in HighlightedFilter.Entities){
                        Remove<HighlightedComponent>(selectedEntity);
                    }
                    Set(entity, new HighlightedComponent());
                }
                if (SomeMessageWithEntity<Select>(entity))
                {
                    foreach (var selectedEntity in SelectedFilter.Entities){
                        Remove<SelectedComponent>(selectedEntity);
                        Remove<HighlightedComponent>(selectedEntity);
                    }
                    Set(entity, new SelectedComponent());
                }
            }
        }
    }
}