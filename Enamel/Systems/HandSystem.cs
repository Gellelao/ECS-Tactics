using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.TempComponents;
using Enamel.Enums;
using Enamel.Spawners;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class HandSystem : MoonTools.ECS.System
{
    private Filter PlayerFilter { get; }
    private Filter ToDiscardFilter { get; }
    
    private readonly OrbSpawner _orbSpawner;

    public HandSystem(World world, OrbSpawner orbSpawner) : base(world)
    {
        _orbSpawner = orbSpawner;
        PlayerFilter = FilterBuilder.Include<PlayerIdComponent>().Build();
        ToDiscardFilter = FilterBuilder.Include<ToBeDiscardedComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<CleanupOrbsInPlayMessage>())
        {
            var playerId = ReadMessage<CleanupOrbsInPlayMessage>().PlayerId;
            var player = GetPlayer(playerId);
            foreach (var orb in OutRelations<OrbInPlayRelation>(player))
            {
                DiscardOrb(player, orb);
                _orbSpawner.DematerializeOrb(orb);
            }
        }

        if (SomeMessage<DrawHandMessage>())
        {
            var message = ReadMessage<DrawHandMessage>();
            var player = GetPlayer(message.PlayerId);
            var numberOfOrbsToDraw = Get<HandSizeComponent>(player).NumberOfOrbsInHand;
            DrawOrbs(player, numberOfOrbsToDraw);
        }
        
        if (SomeMessage<SetStartingOrbsForPlayerMessage>())
        {
            var message = ReadMessage<SetStartingOrbsForPlayerMessage>();
            var player = GetPlayer(message.PlayerId);
            var character = message.CharacterId;
            DestroyExistingOrbsInBag(player);
            AddAllOrbsToBag(player, _orbSpawner.GetStartingOrbsForCharacter(character));
        }

        foreach (var orb in ToDiscardFilter.Entities)
        {
            var playerId = Get<ToBeDiscardedComponent>(orb).PlayerId;
            Remove<ToBeDiscardedComponent>(orb);
            DiscardOrb(GetPlayer(playerId), orb);
        }
    }

    private void AddAllOrbsToBag(Entity player, List<Entity> orbs)
    {
        foreach (var orb in orbs)
        {
            Relate(player, orb, new OrbInBagRelation());
        }
    }

    private void DestroyExistingOrbsInBag(Entity player)
    {
        foreach (var orb in OutRelations<OrbInBagRelation>(player))
        {
            Destroy(orb);
        }
    }

    private void DiscardOrb(Entity player, Entity orb)
    {
        Unrelate<OrbInPlayRelation>(player, orb);
        Relate(player, orb, new OrbInDiscardRelation());
        _orbSpawner.DematerializeOrb(orb);
    }

    private Entity GetPlayer(PlayerId playerId)
    {
        foreach (var player in PlayerFilter.Entities)
        {
            if (Get<PlayerIdComponent>(player).PlayerId == playerId)
            {
                return player;
            }
        }

        throw new Exception($"Could not find player with id {playerId}");
    }

    private void DrawOrbs(Entity player, int numberOfOrbs)
    {
        var existingOrbsInPlay = OutRelationCount<OrbInPlayRelation>(player);
        var handY = Constants.HAND_Y_START - Constants.HAND_ORB_BUFFER * existingOrbsInPlay;
        for (var i = 0; i < numberOfOrbs; i++)
        {
            if (NoOrbsLeftToDraw(player)) return;
            var orb = SelectRandomOrbFromBag(player);
            
            Unrelate<OrbInBagRelation>(player, orb);
            Relate(player, orb, new OrbInPlayRelation());
            
            _orbSpawner.MaterializeOrb(orb, Constants.HAND_X, handY);
            handY -= Constants.HAND_ORB_BUFFER;
        }
    }

    private bool NoOrbsLeftToDraw(Entity player)
    {
        var orbsInBag = OutRelationCount<OrbInBagRelation>(player);
        var orbsInDiscard = OutRelationCount<OrbInDiscardRelation>(player);
        return orbsInBag == 0 && orbsInDiscard == 0;
    }

    private Entity SelectRandomOrbFromBag(Entity player)
    {
        var numberOfOrbsInBag = OutRelationCount<OrbInBagRelation>(player);
        if (numberOfOrbsInBag == 0)
        {
            PutDiscardedOrbsBackInBag(player);
        }
        
        numberOfOrbsInBag = OutRelationCount<OrbInBagRelation>(player);
        return NthOutRelation<OrbInBagRelation>(player, RandomUtils.RandomInt(numberOfOrbsInBag));
    }

    private void PutDiscardedOrbsBackInBag(Entity player)
    {
        foreach (var orb in OutRelations<OrbInDiscardRelation>(player))
        {
            Unrelate<OrbInBagRelation>(player, orb);
            Relate(player, orb, new OrbInBagRelation());
        }
    }
}