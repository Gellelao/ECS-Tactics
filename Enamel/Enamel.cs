using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Systems;
using Enamel.Components;
using Enamel.Renderers;
using FontStashSharp;

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
        static ExampleRenderer? ExampleRenderer;

        SpriteBatch SpriteBatch;
        FontSystem FontSystem;

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
            ExampleRenderer = new ExampleRenderer(World, SpriteBatch, FontSystem);

            /*
            ENTITIES
            */

            for (int i = 0; i < 10; i++)
            {
                var e = World.CreateEntity();
                World.Set<ExampleComponent>(e, new ExampleComponent(0f));
            }

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
            GraphicsDevice.Clear(Color.CornflowerBlue); //set the color of the background. cornflower blue is XNA tradition.

            /*
            call renderers here.
            renderers don't get passed the game time. 
            if you are thinking about passing the game time to a renderer
            in order to do something, try doing it some other way. you'll thank me later.
            */
            ExampleRenderer.Draw();
            base.Draw(gameTime);
        }
    }
}