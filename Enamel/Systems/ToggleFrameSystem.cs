using System;
using Enamel.Components;
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;

namespace Enamel.Systems;

public class ToggleFrameSystem : MoonTools.ECS.System
{
    private Filter ToggleFilter { get; }
    
    public ToggleFrameSystem(World world) : base(world)
    {
        ToggleFilter = FilterBuilder.Include<ToggleFrameOnMouseDownFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        var mouseCurrent = Mouse.GetState();

        foreach (var entity in ToggleFilter.Entities)
        {
            //if(mouseCurrent.)
        }
    }
}