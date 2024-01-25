using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Components;
using FontStashSharp;
using Enamel.Components.UI;
using Enamel.Utils;

namespace Enamel.Renderers;

public class TextRenderer : Renderer
{
    private Filter DebugCoordFilter { get; }
    private Filter TextFilter { get; }
    private SpriteBatch _spriteBatch { get; }
    private readonly SpriteFontBase[] _fonts;

    public TextRenderer(World world, SpriteBatch spriteBatch, SpriteFontBase[] fonts) : base(world)
    {
        _spriteBatch = spriteBatch;
        _fonts = fonts;
        DebugCoordFilter = FilterBuilder
            .Include<DebugCoordComponent>()
            .Build();
        TextFilter = FilterBuilder.Include<TextComponent>().Include<ScreenPositionComponent>().Build();
    }

    public void Draw()
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Matrix.Identity); // Only have to set all these here so I can change the default SamplerState

        // Draw regular text
        foreach (var entity in TextFilter.Entities)
        {
            var (textIndex, fontId, colour) = Get<TextComponent>(entity);
            var text = TextStorage.GetString(textIndex);
            var positionComponent = Get<ScreenPositionComponent>(entity);

            _spriteBatch.DrawString(
                _fonts[(int)fontId],
                text,
                new Vector2(positionComponent.X, positionComponent.Y),
                colour
            );
        }

        // Draw debug text
#if DEBUG
        // foreach (var entity in DebugCoordFilter.Entities)
        // {
        //     var debugCoordComponent = Get<DebugCoordComponent>(entity);
        //     var positionComponent = Get<PositionComponent>(entity);
        //
        //     SpriteBatch.DrawString(
        //         _fontSystem.GetFont(12),
        //         $"{debugCoordComponent.X}, {debugCoordComponent.Y}",
        //         // Give the debug text a bit of a buffer from the corner
        //         new Vector2(positionComponent.X + 7, positionComponent.Y + 5),
        //         Color.Blue
        //     );
        // }
#endif
        _spriteBatch.End();
    }
}