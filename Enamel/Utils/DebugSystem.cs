using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Enamel.Components;
using ImGuiNET;
using MoonTools.ECS;

namespace Enamel.Utils;

public class DebugSystem(World world, Dictionary<int, (IntPtr, Microsoft.Xna.Framework.Vector2)> textures) : MoonTools.ECS.DebugSystem(world)
{
    private bool _showTestWindow;
    private readonly Dictionary<uint, bool> _selectionStatus = new();
    
    public override void Update(TimeSpan delta)
    {
        throw new NotImplementedException();
    }

    public void ImGuiLayout()
    {
        if (ImGui.TreeNode("Entities"))
        {
            if (ImGui.BeginTable("split1", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp))
            {
                foreach (var entity in Debug_GetEntities(typeof(TextureIndexComponent)).Reverse())
                {
                    var selected = _selectionStatus.TryGetValue(entity.ID, out var status) ? status : false;

                    if (ImGui.Selectable(entity.ID.ToString(), selected))
                    {
                        selected = true;
                    }
                    
                    if (selected)
                    {
                        ImGui.SetNextWindowSize(new Vector2(250, 250), ImGuiCond.FirstUseEver);
                        ImGui.Begin(entity.ID.ToString(), ref selected);
                        DrawSpriteForEntity(entity);
                        ShowComponentsForEntity(entity);
                        ImGui.End();
                    }

                    _selectionStatus.TryAdd(entity.ID, selected);
                    _selectionStatus[entity.ID] = selected;
                    
                    DrawSpriteForEntity(entity);
                    
                    ImGui.TableNextColumn();
                }
                ImGui.EndTable();
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
        foreach (var component in Debug_GetAllComponentTypes((Entity) entity))
        {
            // TODO Reflection crimes to show the actual component???
            ImGui.BulletText(component.Name);
        }
    }
}