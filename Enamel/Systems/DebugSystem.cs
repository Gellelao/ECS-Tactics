using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Extensions;
using Enamel.Spawners;
using ImGuiNET;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DebugSystem(
    World world,
    Dictionary<int, (IntPtr, Microsoft.Xna.Framework.Vector2)> textures,
    OrbSpawner orbSpawner)
    : MoonTools.ECS.DebugSystem(world)
{
    private OrbSpawner OrbSpawner { get; } = orbSpawner;
    
    private readonly Dictionary<uint, bool> _selectionStatus = new();
    
    private bool _showTestWindow;
    private int _buttonSize = 50;
    private int _orbSpawnX = 5;

    public override void Update(TimeSpan delta)
    {
        throw new NotImplementedException();
    }

    public void ImGuiLayout()
    {
        ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);
        if (ImGui.TreeNode("Entities"))
        {
            var count = 0;
            ImGui.SliderInt("size", ref _buttonSize, 5, 100);
            var entitiesToShow = Debug_GetEntities(typeof(PlayerIdComponent));
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
                float next_button_x2 =
                    last_button_x2 + style.ItemSpacing.X +
                    _buttonSize; // Expected position if next button was on same line
                if (count + 1 < Debug_GetEntities(typeof(TextureIndexComponent)).Count() &&
                    next_button_x2 < window_visible_x2)
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

        if (ImGui.TreeNode("Spawn"))
        {
            if (ImGui.Button("Reset X"))
            {
                _orbSpawnX = 5;
            }
            if (ImGui.Button("Blue Orb"))
            {
                OrbSpawner.SpawnBlueOrb(_orbSpawnX, 10);
                _orbSpawnX += 15;
            }
            ImGui.SameLine();
            if (ImGui.Button("Grey Orb"))
            {
                OrbSpawner.SpawnColourlessOrb(_orbSpawnX, 10);
                _orbSpawnX += 15;
            }
            ImGui.TreePop();
        }

        if (ImGui.Button("Test Window")) _showTestWindow = !_showTestWindow;
        ImGui.Text(
            $"Application average {1000f / ImGui.GetIO().Framerate:F3} ms/frame ({ImGui.GetIO().Framerate:F1} FPS)"
        );

        if (_showTestWindow)
        {
            ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
            ImGui.ShowDemoWindow(ref _showTestWindow);
        }
    }

    private void ShowEntityDetails(Entity entity, string? name = null)
    {
        DrawSpriteForEntity(entity);
        ShowComponentsForEntity(entity, name);
        ShowRelationsOfEntity(entity);
    }

    private bool DrawButtonForEntity(Entity entity)
    {
        if (!Has<TextureIndexComponent>(entity))
        {
            return ImGui.Button(entity.ID.ToString(), new Vector2(_buttonSize, _buttonSize));
        }
        
        var textureIndex = (int) Get<TextureIndexComponent>(entity).Index;
        var originalTextureWidth = textures[textureIndex].Item2.X;
        var originalTextureHeight = textures[textureIndex].Item2.Y;
        if (Has<SpriteRegionComponent>(entity))
        {
            var subregion = Get<SpriteRegionComponent>(entity);
            // Have to multiply these because of the way animations are stored with each frame being an index in an array
            var subregionX = subregion.X * subregion.Width; 
            var subregionY = subregion.Y * subregion.Height;
            Vector2 uv0 = new Vector2(subregionX / originalTextureWidth, subregionY / originalTextureHeight);
            Vector2 uv1 = new Vector2(
                (subregionX + subregion.Width) / originalTextureWidth,
                (subregionY + subregion.Height) / originalTextureHeight
            );
            return ImGui.ImageButton(
                entity.ID.ToString(),
                textures[textureIndex].Item1,
                new Vector2(_buttonSize, _buttonSize),
                uv0,
                uv1
            );
        }

        return ImGui.ImageButton(
            entity.ID.ToString(),
            textures[textureIndex].Item1,
            new Vector2(_buttonSize, _buttonSize)
        );
    }

    private void DrawSpriteForEntity(Entity entity)
    {
        if (!Has<TextureIndexComponent>(entity)) return;
        var textureIndex = (int) Get<TextureIndexComponent>(entity).Index;
        var originalTextureWidth = textures[textureIndex].Item2.X;
        var originalTextureHeight = textures[textureIndex].Item2.Y;
        if (Has<SpriteRegionComponent>(entity))
        {
            var subregion = Get<SpriteRegionComponent>(entity);
            // Have to multiply these because of the way animations are stored with each frame being an index in an array
            var subregionX = subregion.X * subregion.Width; 
            var subregionY = subregion.Y * subregion.Height;
            Vector2 uv0 = new Vector2(subregionX / originalTextureWidth, subregionY / originalTextureHeight);
            Vector2 uv1 = new Vector2(
                (subregionX + subregion.Width) / originalTextureWidth,
                (subregionY + subregion.Height) / originalTextureHeight
            );
            ImGui.Image(textures[textureIndex].Item1, new Vector2(subregion.Width, subregion.Height), uv0, uv1);
        }
        else
        {
            ImGui.Image(textures[textureIndex].Item1, new Vector2(originalTextureWidth, originalTextureHeight));
        }
    }

    private void ShowComponentsForEntity(Entity? entity, string? name = null)
    {
        if (entity == null) return;
        ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);
        if (ImGui.TreeNode($"Components for entity {name ?? entity.Value.ID.ToString()}"))
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
        if (HasOutRelation<HasSpellRelation>(entity))
        {
            if (ImGui.TreeNode("Spells"))
            {
                foreach (var child in OutRelations<HasSpellRelation>(entity))
                {
                    var spellId = Get<SpellIdComponent>(child).SpellId;
                    var spellName = spellId.ToName();
                    ShowEntityDetails(child, spellName);
                }

                ImGui.TreePop();
            }
        }
        if (HasOutRelation<SocketedRelation>(entity))
        {
            if (ImGui.TreeNode("SocketedEntities"))
            {
                foreach (var child in OutRelations<SocketedRelation>(entity))
                {
                    ShowEntityDetails(child);
                }

                ImGui.TreePop();
            }
        }
        if (HasOutRelation<OrbInBagRelation>(entity))
        {
            if (ImGui.TreeNode("OrbInBagRelation"))
            {
                foreach (var child in OutRelations<OrbInBagRelation>(entity))
                {
                    ShowEntityDetails(child);
                }

                ImGui.TreePop();
            }
        }
        if (HasOutRelation<OrbInPlayRelation>(entity))
        {
            if (ImGui.TreeNode("OrbInPlayRelation"))
            {
                foreach (var child in OutRelations<OrbInPlayRelation>(entity))
                {
                    ShowEntityDetails(child);
                }

                ImGui.TreePop();
            }
        }
        if (HasOutRelation<OrbInDiscardRelation>(entity))
        {
            if (ImGui.TreeNode("OrbInDiscardRelation"))
            {
                foreach (var child in OutRelations<OrbInDiscardRelation>(entity))
                {
                    ShowEntityDetails(child);
                }

                ImGui.TreePop();
            }
        }
    }
}