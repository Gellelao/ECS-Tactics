using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Systems;
using Enamel.Components;
using Enamel.Renderers;
using FontStashSharp;
using System.Collections.Generic;

namespace Enamel
{
    public class Enamel : Game
    {
        GraphicsDeviceManager GraphicsDeviceManager { get; }

        /*
        the World is the place where all our entities go.
        */
        static World World { get; } = new World();
        static ExampleSystem? ExampleSystem;
        static TextureIndexRenderer? TextureIndexRenderer;

        SpriteBatch SpriteBatch;
        FontSystem FontSystem;

        Texture2D[] Textures;

        [STAThread]
        internal static void Main()
        {
            using (Enamel game = new Enamel())
            {
                game.Run();
            }
        }

        private Enamel()
        {
            //setup our graphics device, default window size, etc
            //here is where i will make a plea to you, intrepid game developer:
            //please default your game to windowed mode.
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            GraphicsDeviceManager.PreferredBackBufferWidth = 1024;
            GraphicsDeviceManager.PreferredBackBufferHeight = 768;
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;

            IsFixedTimeStep = false;
            IsMouseVisible = true;
        }

        //you'll want to do most setup in LoadContent() rather than your constructor.
        protected override void LoadContent()
        {
            /*
            CONTENT
            */

            /*
            SpriteBatch is FNA/XNA's abstraction for drawing sprites on the screen.
            you want to do is send all the sprites to the GPU at once, 
            it's much more efficient to send one huge batch than to send sprites piecemeal. 
            See more in the Renderers/ExampleRenderer.cs. 
            */
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            /*
            this is from FontStashSharp. it allows us to load a font and render it at any 
            size we like, complete with effects like blurring and stroke. 
            you can add more than one font to a FontSystem if you have different fonts
            for different languages. FontStashSharp will pick the first font that
            has the characters you're trying to draw, and will otherwise draw the Unicode Tofu (little rectangle)
            */
            FontSystem = new FontSystem();
            FontSystem.AddFont(File.ReadAllBytes(
                    Path.Combine(
                        Content.RootDirectory, "opensans.ttf"
                    )
                ));

            // Unsure if this is the way to do this but keep all textures in a dictionary and refer to them by index?
            Textures = new Texture2D[5];

            var whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            whitePixel.SetData(new Color[] { Color.White });

            var tileTexture = new RenderTarget2D(GraphicsDevice, 80, 80);
            GraphicsDevice.SetRenderTarget(tileTexture);
            SpriteBatch.Begin();
            SpriteBatch.Draw(whitePixel, new Rectangle(0, 0, 80, 80), Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            var groundTexture = Content.Load<Texture2D>("Ground");

            Textures[0] = groundTexture;
            Textures[1] = tileTexture;

            /*
            SYSTEMS
            */

            /*
            here we set up all our systems. 
            you can pass in information that these systems might need to their constructors.
            it doesn't matter what order you create the systems in, we'll specify in what order they run later.
            */
            ExampleSystem = new ExampleSystem(World);

            /*
            RENDERERS
            */

            //same as above, but for the renderer
            TextureIndexRenderer = new TextureIndexRenderer(World, SpriteBatch, Textures);

            /*
            ENTITIES
            */
            
            var e = World.CreateEntity();
            World.Set<TextureIndexComponent>(e, new TextureIndexComponent(0));
            World.Set<PositionComponent>(e, new PositionComponent(100, 100));

            base.LoadContent();
        }

        //sometimes content needs to be unloaded, but it usually doesn't.
        protected override void UnloadContent()
        {
            base.UnloadContent();
        }



        protected override void Update(GameTime gameTime)
        {
            /*
            here we call all our system update functions. 
            call them in the order you want them to run. 
            other ECS libraries have a master "update" function that does this for you,
            but moontools.ecs does not. this lets you have more control
            over the order systems run in, and whether they run at all.
            */

            ExampleSystem.Update(gameTime.ElapsedGameTime);
            World.FinishUpdate(); //always call this at the end of your update function.
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black); //set the color of the background. cornflower blue is XNA tradition.

            /*
            call renderers here.
            renderers don't get passed the game time. 
            if you are thinking about passing the game time to a renderer
            in order to do something, try doing it some other way. you'll thank me later.
            */
            TextureIndexRenderer.Draw();
            base.Draw(gameTime);
        }
    }
}