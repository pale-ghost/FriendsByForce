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
    public class WorkGiver_Warden_BeatSlave : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing target, bool forced = false)
        {
            if (!ShouldTakeCareOfPrisoner_NewTemp(pawn, target, forced))
            {
                return null;
            }
            if (target is Pawn victim && victim.IsSlave(out var slaveComp) && slaveComp.CanBeBeaten && pawn.CanReach(victim, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                Job beatSlave = JobMaker.MakeJob(FBF_DefOf.FBF_BeatSlave, target);
                beatSlave.maxNumMeleeAttacks = Rand.RangeInclusive(3, 5);
                beatSlave.locomotionUrgency = LocomotionUrgency.Jog;
                return beatSlave;
            }
            return null;
        }
    }
}
