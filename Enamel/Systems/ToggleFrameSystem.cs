using System;
using Enamel.Components;
using Enamel.Components.UI;
using Enamel.Utils;
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;

namespace Enamel.Systems;

public class ToggleFrameSystem : MoonTools.ECS.System
{
    private readonly ScreenUtils _screenUtils;

    private readonly AnimationData[] _animations;

    private Filter ToggleOnMouseDownFilter { get; }
    private Filter ToggleOnMouseHoverFilter { get; }
    
    public ToggleFrameSystem(World world, ScreenUtils screenUtils, AnimationData[] animations) : base(world)
    {
        _screenUtils = screenUtils;
        _animations = animations;
        ToggleOnMouseDownFilter = FilterBuilder.Include<ToggleFrameOnMouseDownComponent>().Build();
        ToggleOnMouseHoverFilter = FilterBuilder.Include<ToggleFrameOnMouseHoverComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in ToggleOnMouseHoverFilter.Entities)
        {
            var dimensions = Get<DimensionsComponent>(entity);
            var position = Get<ScreenPositionComponent>(entity);
            var toggleStatus = Get<ToggleFrameOnMouseHoverComponent>(entity);
            var hovered = _screenUtils.MouseInRectangle(position.X, position.Y, dimensions.Width, dimensions.Height);

            switch (toggleStatus.Toggled)
            {
                // Toggle off if currently on and mouse not over the entity
                case true when !hovered:
                    SetEntityFrame(entity, 0);
                    Set(entity, toggleStatus with {Toggled = false});
                    break;
                // Toggle on if currently off and mouse is down
                case false when hovered:
                    SetEntityFrame(entity, toggleStatus.ToggledFrameIndex);
                    Set(entity, toggleStatus with {Toggled = true});
                    break;
            }
        }

        foreach (var entity in ToggleOnMouseDownFilter.Entities)
        {
            var dimensions = Get<DimensionsComponent>(entity);
            var position = Get<ScreenPositionComponent>(entity);
            var toggleStatus = Get<ToggleFrameOnMouseDownComponent>(entity);

            // If mouse is not over the entity, toggle it off
            if(!_screenUtils.MouseInRectangle(position.X, position.Y, dimensions.Width, dimensions.Height))
            {
                if (!toggleStatus.Toggled) continue;
                SetEntityFrame(entity, 0);
                Set(entity, toggleStatus with {Toggled = false});
            }
            else{
                // Mouse is over entity, see if its clicked or not
                var mouseDown = Mouse.GetState().LeftButton == ButtonState.Pressed;

                switch (toggleStatus.Toggled)
                {
                    // Toggle off if currently on and mouse not down
                    case true when !mouseDown:
                        SetEntityFrame(entity, 0);
                        Set(entity, toggleStatus with {Toggled = false});
                        break;
                    // Toggle on if currently off and mouse is down
                    case false when mouseDown:
                        SetEntityFrame(entity, toggleStatus.ToggledFrameIndex);
                        Set(entity, toggleStatus with {Toggled = true});
                        break;
                }
            }
        }
    }

    private void SetEntityFrame(Entity entity, int newFrame){
        var animationSetId = Get<AnimationSetComponent>(entity).AnimationSetId;
        var animationData = _animations[(int)animationSetId];
        Set(entity, new SpriteRegionComponent(
            0, // Assume animations using this system don't have a "facing direction" concept
            newFrame,
            animationData.SpriteWidth,
            animationData.SpriteHeight)
        );
    }
}