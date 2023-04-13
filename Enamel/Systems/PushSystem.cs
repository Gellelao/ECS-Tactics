using System;
using MoonTools.ECS;

namespace Enamel.Systems;

public class PushSystem : MoonTools.ECS.System
{
    public PushSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        throw new NotImplementedException();
    }
}