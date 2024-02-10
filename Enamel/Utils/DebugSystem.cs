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
    
    public override void Update(TimeSpan delta)
    {
        throw new NotImplementedException();
    }

    public void ImGuiLayout()
    {
        if (ImGui.TreeNode("Entities"))
        {
            foreach (var entity in Debug_GetEntities(typeof(TextureIndexComponent)).Reverse())
            {
                DrawSpriteForEntity(entity); ImGui.SameLine();
                if (ImGui.CollapsingHeader(entity.ID.ToString()))
                {
                    ShowComponentsForEntity(entity);
                }
            }
            ImGui.TreePop();
        }

        if (ImGui.Button("Test Window")) _showTestWindow = !_showTestWindow;
        ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate,
            ImGui.GetIO().Framerate));

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