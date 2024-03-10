using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Spawners;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class HandSystem(World world, OrbSpawner orbSpawner) : MoonTools.ECS.System(world)
{
    private const int HAND_X = 10;
    private const int HAND_Y_START = 160;
    private const int ORB_BUFFER = 16;
    private readonly List<Entity> _orbsInPlay = [];

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<CleanupOrbsInPlayMessage>())
        {
            foreach (var orb in _orbsInPlay)
            {
                orbSpawner.DematerializeOrb(orb);
            }
        }

        if (SomeMessage<DrawOrbsMessage>())
        {
            var currentPlayer = GetSingletonEntity<CurrentPlayerFlag>();
            var numberOfOrbs = ReadMessage<DrawOrbsMessage>().NumberOfOrbsToDraw;
            DrawOrbs(currentPlayer, numberOfOrbs);
        }
    }

    private void DrawOrbs(Entity player, int numberOfOrbs)
    {
        var existingOrbsInPlay = OutRelationCount<OrbInPlayRelation>(player);
        var handY = HAND_Y_START - ORB_BUFFER * existingOrbsInPlay;
        for (var i = 0; i < numberOfOrbs; i++)
        {
            var orb = SelectRandomOrbFromBag(player);
            
            Unrelate<OrbInBagRelation>(player, orb);
            Relate(player, orb, new OrbInPlayRelation());
            
            orbSpawner.MaterializeOrb(orb, HAND_X, handY);
            _orbsInPlay.Add(orb);
            handY -= ORB_BUFFER;
        }
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