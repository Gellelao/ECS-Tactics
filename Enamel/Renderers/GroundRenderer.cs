using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Enums;

namespace Enamel.Renderers;

public class GroundRenderer : Renderer
{
    private Filter GroundTextureIndexFilter { get; }
    private SpriteBatch SpriteBatch { get; }
    private readonly Texture2D[] _textures;

    public GroundRenderer(World world, SpriteBatch spriteBatch, Texture2D[] textures) : base(world)
    {
        SpriteBatch = spriteBatch;
        _textures = textures;
        GroundTextureIndexFilter = FilterBuilder
            .Include<TextureIndexComponent>()
            .Include<GroundTileFlag>()
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
        foreach (var entity in GroundTextureIndexFilter.Entities)
        {
            var indexComponent = Get<TextureIndexComponent>(entity);
            var positionComponent = Get<PositionComponent>(entity);

            SpriteBatch.Draw(
                _textures[indexComponent.Index],
                new Vector2(positionComponent.X, positionComponent.Y),
                null,
                Color.White,
                0, // rotation,
                Vector2.Zero, // origin
                Vector2.One, // scaling
                SpriteEffects.None,
                0
            );

            // Draw red origin pixel if in debug mode
#if DEBUG
            SpriteBatch.Draw(
                _textures[(int)Sprite.RedPixel],
                new Vector2(positionComponent.X, positionComponent.Y),
                null,
                Color.White,
                0, // rotation,
                Vector2.Zero, // origin
                Vector2.One, // scaling
                SpriteEffects.None,
                0
            );
#endif
        }
        SpriteBatch.End();
    }
}