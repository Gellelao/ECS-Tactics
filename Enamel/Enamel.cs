using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Systems;
using Enamel.Components;
using Enamel.Renderers;
using FontStashSharp;
using Microsoft.Xna.Framework.Input;

namespace Enamel
{
    public class Enamel : Game
    {
        GraphicsDeviceManager GraphicsDeviceManager { get; }

        /*
        the World is the place where all our entities go.
        */
        static World World { get; } = new World();
        static GridToScreenCoordSystem? GridToScreenCoordSystem;
        static InputSystem? InputSystem;
        static TextureIndexRenderer? TextureIndexRenderer;
        static TextRenderer? TextRenderer;

        SpriteBatch SpriteBatch;
        FontSystem FontSystem;

        Texture2D[] Textures;

        private const int ScreenWidth = 1024;
        private const int ScreenHeight = 768;
        private const int TileWidth = 40;
        private const int TileHeight = 20;
        private const int MapWidth = 8;
        private const int MapHeight = 8;
        private RenderTarget2D _renderTarget;
        private Entity[,] GroundGrid;
        private Entity[,] EntityGrid;


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

            GraphicsDeviceManager.PreferredBackBufferWidth = ScreenWidth;
            GraphicsDeviceManager.PreferredBackBufferHeight = ScreenHeight;
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            GraphicsDeviceManager.PreferMultiSampling = false;

            IsFixedTimeStep = false;
            IsMouseVisible = true;
        }

        //you'll want to do most setup in LoadContent() rather than your constructor.
        protected override void LoadContent()
        {
            _renderTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth/2, ScreenHeight/2);
            GroundGrid = new Entity[MapWidth, MapHeight];
            EntityGrid = new Entity[MapWidth, MapHeight];

            /*
            CONTENT
            */
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            FontSystem = new FontSystem();
            FontSystem.AddFont(File.ReadAllBytes(
                    Path.Combine(
                        Content.RootDirectory, "opensans.ttf"
                    )
                ));

            // Unsure if this is the way to do this but keep all textures in a dictionary and refer to them by index?
            Textures = new Texture2D[5];

            var redPixel = new Texture2D(GraphicsDevice, 1, 1);
            redPixel.SetData(new Color[] { Color.Red });

            Textures[0] = redPixel;
            Textures[1] = Content.Load<Texture2D>("Tile");
            Textures[2] = Content.Load<Texture2D>("Wizard");

            /*
            SYSTEMS
            */

            /*
            here we set up all our systems. 
            you can pass in information that these systems might need to their constructors.
            it doesn't matter what order you create the systems in, we'll specify in what order they run later.
            */
            GridToScreenCoordSystem = new GridToScreenCoordSystem(World, TileWidth, TileHeight, MapWidth, MapHeight);
            InputSystem = new InputSystem(World, TileWidth, TileHeight, MapWidth, MapHeight);

            /*
            RENDERERS
            */

            //same as above, but for the renderer
            TextureIndexRenderer = new TextureIndexRenderer(World, SpriteBatch, Textures);
            TextRenderer = new TextRenderer(World, SpriteBatch, FontSystem);

            /*
            ENTITIES
            */
            
            var player1 = World.CreateEntity();
            var playerSpriteIndex = 2;
            World.Set<TextureIndexComponent>(player1, new TextureIndexComponent(playerSpriteIndex));
            World.Set<SpriteOriginComponent>(player1, new SpriteOriginComponent(Textures[playerSpriteIndex].Width/2, (int)(Textures[playerSpriteIndex].Height*0.8)));
            World.Set<GridCoordComponent>(player1, new GridCoordComponent(3, 2));

            var tileSpriteIndex = 1;
            for(var x = 0; x < 8; x++){
                for(var y = 0;  y < 8; y++){
                    var tile = World.CreateEntity();
                    World.Set<TextureIndexComponent>(tile, new TextureIndexComponent(tileSpriteIndex));
                    World.Set<GridCoordComponent>(tile, new GridCoordComponent(x, y));
                    World.Set<DebugCoordComponent>(tile, new DebugCoordComponent(x, y));
                    World.Set<SpriteOriginComponent>(tile, new SpriteOriginComponent(TileWidth/2, TileHeight/2));
                }
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
            GridToScreenCoordSystem.Update(gameTime.ElapsedGameTime);
            InputSystem.Update(gameTime.ElapsedGameTime);
            World.FinishUpdate(); //always call this at the end of your update function.
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

            /*
            renderers don't get passed the game time. 
            render to the smaller renderTarget, then upscale after
            */
            TextureIndexRenderer.Draw();
            TextRenderer.Draw();

            GraphicsDevice.SetRenderTarget(null);

            SpriteBatch.Begin(SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				SamplerState.PointClamp,
				DepthStencilState.None,
				RasterizerState.CullCounterClockwise,
				null,
				Matrix.Identity); // Only have to set all these here so I can change the default SamplerState
            SpriteBatch.Draw(_renderTarget, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}