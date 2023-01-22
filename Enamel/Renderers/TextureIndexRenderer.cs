using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Components;
using System.Collections.Generic;

namespace Enamel.Renderers
{
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
    public class TextureIndexRenderer : MoonTools.ECS.Renderer
    {
        private Filter TextureIndexFilter { get; }
        private SpriteBatch SpriteBatch { get; }
        private Texture2D[] Textures;

        public TextureIndexRenderer(World world, SpriteBatch spriteBatch, Texture2D[] textures) : base(world)
        {
            SpriteBatch = spriteBatch;
            Textures = textures;
            TextureIndexFilter = FilterBuilder //for information about this, see Systems/ExampleSystem.cs
                .Include<TextureIndexComponent>()
                .Build();
        }

        /*
        note that unlike Update() in systems, Draw() is not an override. 
        this gives us the freedom to have multiple Draw() functions 
        and call them in whatever order we want.
        */
        public void Draw()
        {
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
            SpriteBatch.Begin();
            foreach (var entity in TextureIndexFilter.Entities)
            {
                var indexComponent = Get<TextureIndexComponent>(entity); //getting a component is much like setting a component
                var positionComponent = Get<PositionComponent>(entity);

                SpriteBatch.Draw(
                    Textures[indexComponent.Index],
                    new Vector2(positionComponent.X, positionComponent.Y),
                    null,
                    Color.White,
                    45, // rotation,
                    Vector2.Zero, // origin
                    Vector2.One, // scaling
                    SpriteEffects.None,
                    0
                );
            }
            SpriteBatch.End();
        }
    }
}