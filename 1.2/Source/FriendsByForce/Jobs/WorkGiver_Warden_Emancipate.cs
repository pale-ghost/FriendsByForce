using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FriendsByForce
{
    public class WorkGiver_Warden_Emancipate : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var slave in CompEnslavement.slaves)
            {
                if (slave.Map == pawn.Map)
                {
                    yield return slave;
                }
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return base.HasJobOnThing(pawn, t, forced);
        }
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is Pawn slave && slave.IsSlave(out var slaveComp) && slaveComp.slaverFaction == pawn.Faction && slaveComp.markedForEmancipation && pawn.CanReserve(slave, 1, -1, null, false))
            {
                Job job = JobMaker.MakeJob(FBF_DefOf.FBF_EmancipateSlave, slave);
                return job;
            }
            return null;
        }
    }
}