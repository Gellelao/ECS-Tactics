using Enamel.Components;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Utils;

public class MenuUtils : Manipulator
{
    private Filter DrawLayerFilter { get; }
    
    public MenuUtils(World world) : base(world)
    {
        DrawLayerFilter = FilterBuilder.Include<DrawLayerComponent>().Build();
    }

    public void RecursivelyDestroy(Entity entity)
    {
        var children = OutRelations<IsParentRelation>(entity);
        foreach(var child in children){
            RecursivelyDestroy(child);
        }
        Destroy(entity);
    }

    public void DestroyExistingUiEntities(){
        foreach(var entity in DrawLayerFilter.Entities){
            if (Get<DrawLayerComponent>(entity).Layer == DrawLayer.UserInterface)
            {
                Destroy(entity);
            }
        }
    }

    public Entity CreateRelativeUiEntity(Entity parent, int relativeX, int relativeY, int width, int height){
        var entity = CreateUiEntity(0, 0);
        Relate(parent, entity, new IsParentRelation());
        Set(entity, new DimensionsComponent(width, height));
        Set(entity, new RelativePositionComponent(relativeX, relativeY));
        return entity;
    }

    public Entity CreateUiEntity(int x, int y, int width, int height){
        var entity = CreateUiEntity(x, y);
        Set(entity, new DimensionsComponent(width, height));
        return entity;
    }

    public Entity CreateUiEntity(int x, int y){
        var entity = World.CreateEntity();
        Set(entity, new DrawLayerComponent(DrawLayer.UserInterface));
        Set(entity, new ScreenPositionComponent(x, y));

        return entity;
    }
}