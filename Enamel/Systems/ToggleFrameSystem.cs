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
    
    public ToggleFrameSystem(World world, ScreenUtils screenUtils, AnimationData[] animations) : base(world)
    {
        _screenUtils = screenUtils;
        _animations = animations;
        ToggleOnMouseDownFilter = FilterBuilder.Include<ToggleFrameOnMouseDownComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in ToggleOnMouseDownFilter.Entities)
        {
            var entityDimensions = Get<DimensionsComponent>(entity);
            var entityPosition = Get<ScreenPositionComponent>(entity);
            var toggleStatus = Get<ToggleFrameOnMouseDownComponent>(entity);

            // If mouse is not over the entity, toggle it off
            if(!_screenUtils.MouseInRectangle(entityPosition.X, entityPosition.Y, entityDimensions.Width, entityDimensions.Height)){
                if(toggleStatus.Toggled){
                    SetEntityFrame(entity, 0);
                    Set(entity, new ToggleFrameOnMouseDownComponent(toggleStatus.ToggledFrameIndex, false));
                }
                continue;
            }
            else{
                // Mouse is over entity, see if its clicked or not
                var mouseDown = Mouse.GetState().LeftButton == ButtonState.Pressed;

                // Toggle off if currently on and mouse not down
                if(toggleStatus.Toggled && !mouseDown){
                    SetEntityFrame(entity, 0);
                    Set(entity, new ToggleFrameOnMouseDownComponent(toggleStatus.ToggledFrameIndex, false));
                }

                // Toggle on if currently off and mouse is down
                if(!toggleStatus.Toggled && mouseDown){
                    SetEntityFrame(entity, toggleStatus.ToggledFrameIndex);
                    Set(entity, new ToggleFrameOnMouseDownComponent(toggleStatus.ToggledFrameIndex, true));
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