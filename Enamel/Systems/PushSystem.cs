using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class PushSystem : MoonTools.ECS.System
{
    private Filter PushableFilter { get; }

    public PushSystem(World world) : base(world)
    {
        PushableFilter = FilterBuilder.Include<PushableFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var pushable in PushableFilter.Entities)
        {
            var pushMessages = ReadMessagesWithEntity<PushMessage>(pushable);

            foreach (var pushMessage in pushMessages)
            {
                Push(pushable, pushMessage.Direction);
            }
        }
    }

    private void Push(Entity entity, Direction direction)
    {
        throw new NotImplementedException();
    }
}