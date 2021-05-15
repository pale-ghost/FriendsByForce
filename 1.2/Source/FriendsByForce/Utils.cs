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
    [StaticConstructorOnStartup]
    public static class Utils
    {
        static Utils()
        {
            foreach (var human in DefDatabase<ThingDef>.AllDefs.Where(x => x.race?.Humanlike ?? false))
            {
                human.comps.Add(new CompProperties_Enslavement());
            }
            DefDatabase<DesignationCategoryDef>.GetNamed("Zone").AllResolvedDesignators.Add(new Designator_AreaSlaveLaborExpand());
            DefDatabase<DesignationCategoryDef>.GetNamed("Zone").AllResolvedDesignators.Add(new Designator_AreaSlaveLaborClear());
        }
        public static bool IsSlaveCollar(this Thing thing)
        {
            return thing.def.IsSlaveCollar();
        }
        public static bool IsSlaveCollar(this ThingDef thingDef)
        {
            return FBF_DefOf.FBF_RopeCollar == thingDef;
        }

        private static Dictionary<Pawn, CompEnslavement> cachedComps = new Dictionary<Pawn, CompEnslavement>();
        public static CompEnslavement GetCachedSlaveComp(this Pawn pawn)
        {
            if (!cachedComps.TryGetValue(pawn, out var comp) || comp is null)
            {
                comp = pawn.TryGetComp<CompEnslavement>();
                cachedComps[pawn] = comp;
            }
            return comp;
        }

        public static bool IsSlave(this Pawn pawn)
        {
            return IsSlave(pawn, out CompEnslavement slaveComp);
        }
        public static bool IsSlave(this Pawn pawn, out CompEnslavement slaveComp)
        {
            slaveComp = pawn.GetCachedSlaveComp();
            if (slaveComp != null)
            {
                return slaveComp.isSlave;
            }
            return false;
        }

        public static bool HasSlaveCollar(this Pawn pawn, out Apparel slaveCollar)
        {
            slaveCollar = null;
            if (pawn.apparel == null)
            {
                return false;
            }
            foreach (Apparel item in pawn.apparel.WornApparel)
            {
                if (IsSlaveCollar(item))
                {
                    slaveCollar = item;
                    return true;
                }
            }
            return false;
        }

        public static bool CanUseIt(this Pawn pawn, Thing thing)
        {
            if (pawn.IsSlave(out var slaveComp))
            {
                var slaveLaborArea = pawn.Map?.areaManager?.Get<Area_SlaveLabor>();
                if (slaveLaborArea != null)
                {
                    return slaveLaborArea[thing.PositionHeld];
                }
            }
            return true;
        }

        public static void Damage(this Thing thing, float hpPctToDamage)
        {
            var hitpointsToTake = (int)((hpPctToDamage / 1f) * thing.MaxHitPoints);
            thing.HitPoints -= hitpointsToTake;
            if (thing.HitPoints <= 0)
            {
                thing.Destroy();
            }
        }

        public static Job EscapeJob(Pawn pawn, bool breakCollarFirst, bool forceCanDig, bool canBash, bool forceCanDigIfCantReachMapEdge, bool forceCanDigIfAnyHostileActiveThreat)
        {
            bool flag = forceCanDig || (pawn.mindState.duty != null && pawn.mindState.duty.canDig && !pawn.CanReachMapEdge()) || (forceCanDigIfCantReachMapEdge && !pawn.CanReachMapEdge()) || (forceCanDigIfAnyHostileActiveThreat && pawn.Faction != null && GenHostility.AnyHostileActiveThreatTo(pawn.Map, pawn.Faction, countDormantPawnsAsHostile: true));
            if (!TryFindGoodExitDest(pawn, flag, out IntVec3 dest))
            {
                return null;
            }
            var slaveComp = pawn.GetCachedSlaveComp();
            slaveComp.lastEscapeAttemptTick = Find.TickManager.TicksGame;
            if (breakCollarFirst && pawn.HasSlaveCollar(out var slaveCollar))
            {
                var manipulationLevel = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
                slaveCollar.Damage(0.1f * manipulationLevel);
                if (!slaveCollar.DestroyedOrNull())
                {
                    return null;
                }
            }
            if (flag)
            {
                using (PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, dest, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings)))
                {
                    IntVec3 cellBefore;
                    Thing thing = path.FirstBlockingBuilding(out cellBefore, pawn);
                    if (thing != null)
                    {
                        Job job = DigUtility.PassBlockerJob(pawn, thing, cellBefore, canMineMineables: true, canMineNonMineables: true);
                        if (job != null)
                        {
                            return job;
                        }
                    }
                }
            }
            Job job2 = JobMaker.MakeJob(JobDefOf.Goto, dest);
            job2.exitMapOnArrival = true;
            job2.locomotionUrgency = LocomotionUrgency.Jog;
            job2.expiryInterval = 999999;
            job2.canBash = canBash;
            slaveComp.EscapeFromSlavers();
            Find.LetterStack.ReceiveLetter("ESCAPE", "TEST", LetterDefOf.NegativeEvent, pawn);
            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
            return job2;
        }
        private static bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 spot)
        {
            TraverseMode mode = canDig ? TraverseMode.PassAllDestroyableThings : TraverseMode.ByPawn;
            return RCellFinder.TryFindBestExitSpot(pawn, out spot, mode);
        }
    }
}
