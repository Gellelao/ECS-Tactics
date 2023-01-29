using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Components;
using FontStashSharp;
using System;

namespace Enamel.Renderers
{
    public class TextRenderer : MoonTools.ECS.Renderer
    {
        private Filter DebugCoordFilter { get; }
        private SpriteBatch SpriteBatch { get; }
        private FontSystem _fontSystem;

        public TextRenderer(World world, SpriteBatch spriteBatch, FontSystem fontSystem) : base(world)
        {
            SpriteBatch = spriteBatch;
            _fontSystem = fontSystem;
            DebugCoordFilter = FilterBuilder
                .Include<DebugCoordComponent>()
                .Build();
        }

        public void Draw()
        {
            SpriteBatch.Begin(SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				SamplerState.PointClamp,
				DepthStencilState.None,
				RasterizerState.CullCounterClockwise,
				null,
				Matrix.Identity); // Only have to set all these here so I can change the default SamplerState

            #if DEBUG
            foreach (var entity in DebugCoordFilter.Entities)
            {
                var debugCoordComponent = Get<DebugCoordComponent>(entity);
                var positionComponent = Get<PositionComponent>(entity);

                SpriteBatch.DrawString(
                    _fontSystem.GetFont(12),
                    String.Format("{0}, {1}", debugCoordComponent.X, debugCoordComponent.Y),
                    new Vector2(positionComponent.X, positionComponent.Y),
                    Color.Blue
                );
            }
            #endif
            SpriteBatch.End();
        }
    }
}