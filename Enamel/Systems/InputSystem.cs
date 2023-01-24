using System;
using MoonTools.ECS;
using Microsoft.Xna.Framework.Input;

namespace Enamel.Systems
{
    public class InputSystem : MoonTools.ECS.System
    {

        public InputSystem(World world) : base(world)
        {
        }

        public override void Update(TimeSpan delta)
        {
            var mouseState = Mouse.GetState();
        }
    }
}