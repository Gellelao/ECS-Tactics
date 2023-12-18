﻿using System;
using Enamel.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Enamel;

public static class ContentUtils
{
    public static Texture2D[] LoadTextures(ContentManager content, GraphicsDevice graphicsDevice,
        SpriteBatch spriteBatch)
    {
        var textures = new Texture2D[100];
        
        var redPixel = new Texture2D(graphicsDevice, 1, 1);
        redPixel.SetData(new[] { Color.Red });

        var greenPixel = new Texture2D(graphicsDevice, 1, 1);
        greenPixel.SetData(new[] { Color.ForestGreen });
        var greenRectangle = new RenderTarget2D(graphicsDevice, 40, 20);
        graphicsDevice.SetRenderTarget(greenRectangle);
        spriteBatch.Begin();
        spriteBatch.Draw(greenPixel, new Rectangle(0, 0, 40, 20), Color.White);
        spriteBatch.End();
        graphicsDevice.SetRenderTarget(null);

        var yellowPixel = new Texture2D(graphicsDevice, 1, 1);
        yellowPixel.SetData(new[] { Color.Yellow });
        var yellowSquare = new RenderTarget2D(graphicsDevice, 30, 30);
        graphicsDevice.SetRenderTarget(yellowSquare);
        spriteBatch.Begin();
        spriteBatch.Draw(yellowPixel, new Rectangle(0, 0, 30, 30), Color.White);
        spriteBatch.End();
        graphicsDevice.SetRenderTarget(null);

        textures[(int)Sprite.RedPixel] = redPixel;
        textures[(int)Sprite.GreenRectangle] = greenRectangle;
        textures[(int)Sprite.YellowSquare] = yellowSquare;
        textures[(int)Sprite.Tile] = content.Load<Texture2D>("GroundTile");
        textures[(int)Sprite.GreenCube] = content.Load<Texture2D>("greenCube");
        textures[(int)Sprite.BlueWizard] = content.Load<Texture2D>("blueWiz");
        textures[(int)Sprite.YellowCube] = content.Load<Texture2D>("yellowCube");
        textures[(int)Sprite.SelectedTile] = content.Load<Texture2D>("Selected");
        textures[(int)Sprite.TileSelectPreview] = content.Load<Texture2D>("TilePreview");
        textures[(int)Sprite.Fireball] = content.Load<Texture2D>("fireball");
        textures[(int)Sprite.ArcaneBlock] = content.Load<Texture2D>("ArcaneCube");
        textures[(int)Sprite.ArcaneBubble] = content.Load<Texture2D>("bubble");
        textures[(int)Sprite.Smoke] = content.Load<Texture2D>("SmokePuff");
        textures[(int)Sprite.TitleScreen] = content.Load<Texture2D>("TitleScreen");

        return textures;
    }

    public static AnimationData[] LoadAnimations()
    {
        var animations = new AnimationData[100];
        // X and Y are the coords of the segment of the sprite sheet we want to draw, if each sprite was a cell in an array
        // we'll multiply X and Y by the size of the sprite to get the pixel coords when rendering.
        // Here we are only defining arrays of Y values, because X is determined by the direction of the sprite (see sprite sheet, each column has all the sprites for once direction)
        var blueWizAnimations = new int[Enum.GetNames(typeof(AnimationType)).Length][];
        blueWizAnimations[(int)AnimationType.Idle] = [1];
        blueWizAnimations[(int)AnimationType.Walk] = [0, 1, 2, 1];
        blueWizAnimations[(int)AnimationType.Hurt] = [3];
        blueWizAnimations[(int)AnimationType.Raise] = [4];
        blueWizAnimations[(int)AnimationType.Throw] = [5];
        animations[(int) AnimationSet.BlueWiz] = new AnimationData(
            Constants.PLAYER_FRAME_WIDTH,
            Constants.PLAYER_FRAME_HEIGHT, 
            blueWizAnimations
        );
        
        animations[(int) AnimationSet.Smoke] = new AnimationData(15, 18, [[0, 1, 2, 3]]);

        return animations;
    }
}