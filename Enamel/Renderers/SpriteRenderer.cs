using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Enums;

namespace Enamel.Renderers;

public class SpriteRenderer : Renderer
{
    private Filter TextureIndexFilter { get; }
    private SpriteBatch SpriteBatch { get; }
    private readonly Texture2D[] _textures;

    public SpriteRenderer(World world, SpriteBatch spriteBatch, Texture2D[] textures) : base(world)
    {
        SpriteBatch = spriteBatch;
        _textures = textures;
        // An alternative here would be to give each draw layer its own component type, then create a separate filter
        // for each. It would save some of the GetRenderOrder complexity but we need to order by Y value anyway
        TextureIndexFilter = FilterBuilder
            .Include<TextureIndexComponent>()
            .Include<ScreenPositionComponent>()
            .Include<DrawLayerComponent>()
            .Build();
    }

    public void Draw()
    {
        var renderOrder = GetRenderOrder(TextureIndexFilter.Entities);

        SpriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Matrix.Identity); // Only have to set all these here so I can change the default SamplerState

        foreach (var entityList in renderOrder)
        {
            foreach (var entity in entityList)
            {
                DrawEntity(entity);
            }
        }
        SpriteBatch.End();
    }
    
    /// <summary>
    /// Given a list of entities with PostitionCompnents and DrawLayerComponents, return them ordered such that:
    /// * All entities in lower layers are drawn before entities in higher layers
    /// * Within each layer, entities at lower(?) Y values are drawn before entities at higher(?) Y values
    /// * Also ensure that parents are rendered before children, breaking the y value ordering if necessary
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    private IEnumerable<List<Entity>> GetRenderOrder(ReverseSpanEnumerator<Entity> entities)
    {
        // Create a list with one entry for each draw layer
        var layerList = new List<List<Entity>>();
        foreach (int currentLayer in Enum.GetValues(typeof(DrawLayer)))
        {
            var currentLayerEnum = (DrawLayer) currentLayer;

            var entitiesForLayer = new List<Entity>();
            foreach (var entity in entities)
            {
                var drawLayer = Get<DrawLayerComponent>(entity).Layer;
                if (drawLayer == currentLayerEnum)
                {
                    entitiesForLayer.Add(entity);
                }
            }

            layerList.Add(GetRenderOrderForLayer(entitiesForLayer));
        }
        
        // Combine the draw layer lists into one big list. This results in the first item being all the entities to
        // draw first, the second item being all the entities to draw second, etc. Taking into account both draw layer
        // and Y value.
        return layerList;
    }

    private List<Entity> GetRenderOrderForLayer(List<Entity> entities)
    {
        // Create a dictionary with lists of entities at each Y level
        var renderOrderDict = new Dictionary<int, List<Entity>>();
        foreach (var entity in entities)
        {
            
            var yPos = Get<ScreenPositionComponent>(entity).Y;
            if (renderOrderDict.TryGetValue(yPos, out var list))
            {
                list.Add(entity);
            }
            else
            {
                renderOrderDict.Add(yPos, new List<Entity>{ entity });
            }
        }

        // Order by Y value and select out just the list of ordered entities
        var yOrderedList = renderOrderDict
            .OrderBy(kvp => kvp.Key)
            .Reverse()
            .Select(kvp => kvp.Value)
            .SelectMany(list => list);

        // Now put parents before children. This may break the Y-sorting but thats ok
        return HierarchicalSort(yOrderedList);
    }

    private List<Entity> HierarchicalSort(IEnumerable<Entity> entities)
    {
        var visited = new HashSet<Entity>();
        var sortedList = new List<Entity>();

        foreach (var entity in entities)
        {
            if (!visited.Contains(entity))
            {
                Dfs(entity, visited, sortedList);
            }
        }

        sortedList.Reverse();

        return sortedList;
    }

    private void Dfs(Entity entity, HashSet<Entity> visited, List<Entity> sortedList)
    {
        visited.Add(entity);

        var children = OutRelations<IsParentRelation>(entity);

        foreach (var child in children)
        {
            if (!visited.Contains(child))
            {
                Dfs(child, visited, sortedList);
            }
        }

        sortedList.Add(entity);
    }

    private void DrawEntity(Entity entity)
    {
            var indexComponent = Get<TextureIndexComponent>(entity);
            var positionComponent = Get<ScreenPositionComponent>(entity);

            var origin = Vector2.Zero;
            if(Has<SpriteOriginComponent>(entity))
            {
                var originComponent = Get<SpriteOriginComponent>(entity);
                origin = new Vector2(originComponent.X, originComponent.Y);
            }

            Rectangle? sourceRect = null;
            if (Has<SpriteRegionComponent>(entity))
            {
                var rect = Get<SpriteRegionComponent>(entity);
                sourceRect = rect.ToRectangle();
            }

            var tint = Color.White;
            if(Has<HighlightedFlag>(entity))
            {
                tint = Constants.HighlightColour;
            }

            // Draw the selection square under the unit if selected
            if(Has<SelectedFlag>(entity)){
                SpriteBatch.Draw(
                    _textures[(int)Sprite.SelectedTile],
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
            // SpriteBatch.Draw(
            //     _textures[(int)Sprite.RedPixel],
            //     new Vector2(positionComponent.X, positionComponent.Y),
            //     null,
            //     Color.White,
            //     0, // rotation,
            //     Vector2.Zero, // origin
            //     Vector2.One, // scaling
            //     SpriteEffects.None,
            //     0
            // );
#endif
    }
}