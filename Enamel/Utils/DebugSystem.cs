using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Enums;
using ImGuiNET;
using MoonTools.ECS;

namespace Enamel.Utils;

public class DebugSystem(World world, Dictionary<int, (IntPtr, Microsoft.Xna.Framework.Vector2)> textures) : MoonTools.ECS.DebugSystem(world)
{
    private bool _showTestWindow;
    private readonly Dictionary<uint, bool> _selectionStatus = new();
    private int _buttonSize = 50;
    
    public override void Update(TimeSpan delta)
    {
        throw new NotImplementedException();
    }

    public void ImGuiLayout()
    {
        if (ImGui.TreeNode("Entities with Sprites"))
        {
            var count = 0;
            ImGui.SliderInt("size", ref _buttonSize, 5, 100);
            var entitiesToShow = Debug_GetEntities(typeof(PlayerNumberComponent));
            entitiesToShow = entitiesToShow.Concat(Debug_GetEntities(typeof(TextureIndexComponent)).Reverse());
            foreach (var entity in entitiesToShow)
            {
                var selected = _selectionStatus.TryGetValue(entity.ID, out var status) ? status : false;
                
                if (DrawButtonForEntity(entity))
                {
                    selected = true;
                }
                var window_visible_x2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                var style = ImGui.GetStyle();
                float last_button_x2 = ImGui.GetItemRectMax().X;
                float next_button_x2 = last_button_x2 + style.ItemSpacing.X + _buttonSize; // Expected position if next button was on same line
                if (count + 1 < Debug_GetEntities(typeof(TextureIndexComponent)).Count() && next_button_x2 < window_visible_x2)
                    ImGui.SameLine();
                    
                if (selected)
                {
                    ImGui.SetNextWindowSize(new Vector2(300, 400), ImGuiCond.FirstUseEver);
                    ImGui.Begin(entity.ID.ToString(), ref selected, ImGuiWindowFlags.AlwaysAutoResize);
                    ShowEntityDetails(entity);
                    ImGui.End();
                }

                _selectionStatus.TryAdd(entity.ID, selected);
                _selectionStatus[entity.ID] = selected;

                count++;
            }
            ImGui.TreePop();
        }

        if (ImGui.Button("Test Window")) _showTestWindow = !_showTestWindow;
        ImGui.Text(
            $"Application average {1000f / ImGui.GetIO().Framerate:F3} ms/frame ({ImGui.GetIO().Framerate:F1} FPS)");

        if (_showTestWindow)
        {
            ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
            ImGui.ShowDemoWindow(ref _showTestWindow);
        }
    }

    private void ShowEntityDetails(Entity entity)
    {
        DrawSpriteForEntity(entity);
        ShowComponentsForEntity(entity);
        ShowRelationsOfEntity(entity);
    }

    private bool DrawButtonForEntity(Entity entity)
    {
        if (!Has<TextureIndexComponent>(entity))
        {
            return ImGui.Button(entity.ID.ToString(), new Vector2(_buttonSize, _buttonSize));
        };
        var textureIndex = (int)Get<TextureIndexComponent>(entity).Index;
        var originalTextureWidth = textures[textureIndex].Item2.X;
        var originalTextureHeight = textures[textureIndex].Item2.Y;
        if (Has<SpriteRegionComponent>(entity))
        {
            var subregion = Get<SpriteRegionComponent>(entity);
            var subregionX = subregion.X * subregion.Width; // Have to multiply these because of the way animations are stored with each frame being an index in an array
            var subregionY = subregion.Y * subregion.Height;
            Vector2 uv0 = new Vector2(subregionX / originalTextureWidth, subregionY / originalTextureHeight);
            Vector2 uv1 = new Vector2((subregionX+subregion.Width) / originalTextureWidth, (subregionY+subregion.Height) / originalTextureHeight);
            return ImGui.ImageButton(entity.ID.ToString(), textures[textureIndex].Item1, new Vector2(_buttonSize, _buttonSize), uv0, uv1);
        }

        return ImGui.ImageButton(entity.ID.ToString(), textures[textureIndex].Item1, new Vector2(_buttonSize, _buttonSize));
    }
    
    private void DrawSpriteForEntity(Entity entity)
    {
        if (!Has<TextureIndexComponent>(entity)) return;
        var textureIndex = (int)Get<TextureIndexComponent>(entity).Index;
        var originalTextureWidth = textures[textureIndex].Item2.X;
        var originalTextureHeight = textures[textureIndex].Item2.Y;
        if (Has<SpriteRegionComponent>(entity))
        {
            var subregion = Get<SpriteRegionComponent>(entity);
            var subregionX = subregion.X * subregion.Width; // Have to multiply these because of the way animations are stored with each frame being an index in an array
            var subregionY = subregion.Y * subregion.Height;
            Vector2 uv0 = new Vector2(subregionX / originalTextureWidth, subregionY / originalTextureHeight);
            Vector2 uv1 = new Vector2((subregionX+subregion.Width) / originalTextureWidth, (subregionY+subregion.Height) / originalTextureHeight);
            ImGui.Image(textures[textureIndex].Item1, new Vector2(subregion.Width, subregion.Height), uv0, uv1);
        }
        else
        {
            ImGui.Image(textures[textureIndex].Item1, new Vector2(originalTextureWidth, originalTextureHeight));
        }
    }

    private void ShowComponentsForEntity(Entity? entity)
    {
        if (entity == null) return;
        ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);
        if (ImGui.TreeNode($"Components for entity {entity.Value.ID.ToString()}"))
        {
            foreach (var component in Debug_GetAllComponentTypes((Entity) entity))
            {
                // TODO Reflection crimes to show the actual component???
                ImGui.BulletText(component.Name);
            }
            ImGui.TreePop();
        }
    }

    private void ShowRelationsOfEntity(Entity entity)
    {
        if (HasOutRelation<IsParentRelation>(entity))
        {
            if (ImGui.TreeNode("Children"))
            {
                foreach (var child in OutRelations<IsParentRelation>(entity))
                {
                    ShowEntityDetails(child);
                }
                ImGui.TreePop();
            }
        }
    }
}