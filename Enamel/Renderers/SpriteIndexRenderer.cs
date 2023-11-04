using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Enums;

namespace Enamel.Renderers;

/*
this is a renderer. a renderer is just like a system, 
but it lacks the capability to call Set() and update components.
*never* update components from within a renderer.

how you structure your renderers is up to you. 
you may get value from splitting your renderers up into say,
a HUD renderer and a world renderer. or you can go monolith-style.
there are advantages to both ([lcd soundsystem voice] advantages! advantages!),
but the monolith-style renderer can be more useful in a 3D context.
*/
public class SpriteIndexRenderer : Renderer
{
    private Filter TextureIndexFilter { get; }
    private SpriteBatch SpriteBatch { get; }
    private readonly Texture2D[] _textures;

    public SpriteIndexRenderer(World world, SpriteBatch spriteBatch, Texture2D[] textures) : base(world)
    {
        SpriteBatch = spriteBatch;
        _textures = textures;
        TextureIndexFilter = FilterBuilder
            .Include<TextureIndexComponent>()
            .Include<PositionComponent>()
            .Exclude<GroundTileFlag>()
            .Build();
    }

    /*
    note that unlike Update() in systems, Draw() is not an override. 
    this gives us the freedom to have multiple Draw() functions 
    and call them in whatever order we want.
    */
    public void Draw()
    {
        var renderOrderDict = GetRenderOrder(TextureIndexFilter.Entities);
        var orderedList = renderOrderDict.OrderBy(kvp => kvp.Key).ToList();
        /*
        start the sprite batch. everything between SpriteBatch.Begin() and SpriteBatch.End() 
        is going to be in the same batch. everything that gets batched together has to use 
        the same shader, so if you have a special shader you want to put on certain objects only,
        they have to be in their own batch.

        it's also good practice to create a sprite atlas using a texture packer, 
        and draw all your sprites from the same Texture2D, specifying which sprite you want using
        the rectangle parameter. there are many tools out there that will spit out a packed texture
        and JSON metadata to get your rectangles from. i recommend cram: https://gitea.moonside.games/MoonsideGames/Cram
        */
        SpriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Matrix.Identity); // Only have to set all these here so I can change the default SamplerState
        foreach (var kvp in orderedList)
        {
            foreach (var entity in kvp.Value)
            {
                DrawEntity(entity);
            }
        }
        SpriteBatch.End();
    }

    private Dictionary<int, List<Entity>> GetRenderOrder(ReverseSpanEnumerator<Entity> entities)
    {
        var renderOrderDict = new Dictionary<int, List<Entity>>();
        foreach (var entity in entities)
        {
            var yPos = Get<PositionComponent>(entity).Y;
            if (renderOrderDict.TryGetValue(yPos, out var list))
            {
                list.Add(entity);
            }
            else
            {
                renderOrderDict.Add(yPos, new List<Entity>{ entity });
            }
        }
        return renderOrderDict;
    }

    private void DrawEntity(Entity entity)
    {
            var indexComponent = Get<TextureIndexComponent>(entity);
            var positionComponent = Get<PositionComponent>(entity);

            var origin = Vector2.Zero;
            if(Has<SpriteOriginComponent>(entity))
            {
                var originComponent = Get<SpriteOriginComponent>(entity);
                origin = new Vector2(originComponent.X, originComponent.Y);
            }

            Rectangle? sourceRect = null;
            if (Has<SpriteRectComponent>(entity))
            {
                var rect = Get<SpriteRectComponent>(entity);
                sourceRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }

            var tint = Color.White;
            if(Has<HighlightedFlag>(entity))
            {
                tint = Constants.HighlightColour;
            }

            // Draw the selection square under the unit if selected
            if(Has<SelectedFlag>(entity)){
                SpriteBatch.Draw(
                    _textures[(int)Sprite.Selection],
                    positionComponent.ToVector,
                    null,
                    Color.White,
                    0, // rotation,
                    Vector2.Zero, // origin
                    Vector2.One, // scaling
                    SpriteEffects.None,
                    0
                );
            }

            // Draw the unit itself
            SpriteBatch.Draw(
                _textures[(int)indexComponent.Index],
                positionComponent.ToVector,
                sourceRect,
                tint,
                0, // rotation,
                origin, // origin
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
}