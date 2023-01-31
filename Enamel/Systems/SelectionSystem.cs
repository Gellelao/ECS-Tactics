using System;
using MoonTools.ECS;
using Enamel.Components;
using Microsoft.Xna.Framework;
using Enamel.Components.Messages;

namespace Enamel.Systems
{
    public class SelectionSystem : MoonTools.ECS.System
    {
        private Filter _selectableFilter { get; }
        private Filter _selectedFilter { get; }

        public SelectionSystem(World world) : base(world)
        {
            _selectableFilter = FilterBuilder
                .Include<SelectableComponent>()
                .Build();
            _selectedFilter = FilterBuilder
                .Include<SelectedComponent>()
                .Build();
        }

        public override void Update(TimeSpan delta)
        {
            foreach (var entity in _selectableFilter.Entities)
            {
                if (SomeMessageWithEntity<Select>(entity))
                {
                    foreach (var selectedEntity in _selectedFilter.Entities){
                        Remove<SelectedComponent>(selectedEntity);
                    }
                    Set(entity, new SelectedComponent());
                }
            }
        }
    }
}