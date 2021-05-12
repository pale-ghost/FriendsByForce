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
    [HarmonyPatch(typeof(ITab_Pawn_Visitor), "FillTab")]
    public static class ITab_PawnVisitor_FillTab_Patch
    {
        public static bool Prepare()
        {
            return ModLister.GetModWithIdentifier("avius.prisonlabor") is null;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    	{
            MethodInfo methodInfo = typeof(ITab_PawnVisitor_FillTab_Patch).GetMethod("PrisonerInteractionModeDefCount");
            foreach (CodeInstruction inst in instructions)
            {
                if (inst.opcode == OpCodes.Ldc_R4 && inst.operand.ToStringSafe() == "160")
                {
                    yield return new CodeInstruction(OpCodes.Call, methodInfo);
                }
                else
                    yield return inst;
            }
        }

    	public static float PrisonerInteractionModeDefCount()
    	{
    		return DefDatabase<PrisonerInteractionModeDef>.DefCount * 32f;
    	}
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain_NewTmp")]
    public static class Patch_ApparelScoreGain_NewTmp
    {
        private static bool Prefix(ref float __result, Pawn pawn, Apparel ap, List<float> wornScoresCache)
        {
            if (pawn.IsSlave() && ap != null && ap.IsSlaveCollar() && pawn.HasSlaveCollar())
            {
                __result = -1000f;
                return false;
            }
            else if (!pawn.IsSlave() && pawn.IsColonist && ap != null && ap.IsSlaveCollar())
            {
                __result = -1000f;
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
	public static class OptimizeApparel_Patch
	{
        public static void Prefix(Pawn pawn, out bool __state)
        {
            if (pawn.IsSlave() && pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
                __state = true;
            }
            else
            {
                __state = false;
            }
        }
        public static void Postfix(Pawn pawn, ref Job __result, bool __state)
        {
            if (__result != null)
            {
                if (pawn.IsSlave() && __result.targetA.Thing != null && __result.targetA.Thing.IsSlaveCollar() && pawn.HasSlaveCollar())
                {
                    __result = null;
                }
                else if (!pawn.IsSlave() && pawn.IsColonist && __result.targetA.Thing != null && __result.targetA.Thing.IsSlaveCollar())
                {
                    __result = null;
                }
            }

            if (__state && pawn.IsSlave(out CompEnslavement comp) && comp.previousFaction != pawn.Faction)
            {
                pawn.SetFaction(comp.previousFaction);
            }
        }
	}

    [HarmonyPatch(typeof(Pawn), "IsColonist", MethodType.Getter)]
    public static class IsColonist_Patch
    {
        private static void Postfix(Pawn __instance, ref bool __result)
        {
            if (!__result && __instance.RaceProps.Humanlike && __instance.IsSlave())
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "IsColonistPlayerControlled", MethodType.Getter)]
    public static class IsColonistPlayerControlled_Patch
    {
        private static void Postfix(Pawn __instance, ref bool __result)
        {
            if (__result && __instance.RaceProps.Humanlike && __instance.IsSlave())
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(PawnNameColorUtility), "PawnNameColorOf")]
    public static class Patch_PawnNameColorOf
    {
        private static void Postfix(ref Color __result, Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike && pawn.IsSlave())
            {
                __result = Color.gray;
            }
        }
    }

    [HarmonyPatch(typeof(ThinkNode_ConditionalNeedPercentageAbove), "Satisfied")]
    public static class Patch_Satisfied
    {
        private static bool Prefix(Pawn pawn, NeedDef ___need)
        {
            if (___need == NeedDefOf.Joy && pawn.IsSlave())
            {
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(MainTabWindow_PawnTable), "Pawns", MethodType.Getter)]
    public static class Patch_Pawns
    {
        private static IEnumerable<Pawn> Postfix(IEnumerable<Pawn> __result)
        {
            foreach (var r in __result)
            {
                yield return r;
            }

            foreach (var pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
            {
                if (pawn.IsSlave(out var slaveComp) && slaveComp.slaverFaction == Faction.OfPlayer)
                {
                    yield return pawn;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ReservationManager), "CanReserve")]
    public static class Patch_CanReserve
    {
        private static void Postfix(ref bool __result, Pawn claimant, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
        {
            if (__result)
            {
                if (target.HasThing && !claimant.CanUseIt(target.Thing))
                {
                    __result = false;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(Thing), typeof(Pawn) })]
    public static class Patch_IsForbidden
    {
        private static void Postfix(ref bool __result, Thing t, Pawn pawn)
        {
            if (!__result)
            {
                if (!pawn.CanUseIt(t))
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GenHostility), "HostileTo", new Type[]
    {
        typeof(Thing),
        typeof(Thing)
    })]
    public static class HostileTo_Patch
    {
        public static void Postfix(Thing a, Thing b, ref bool __result)
        {
            CompEnslavement slaveComp1;
            CompEnslavement slaveComp2;
            if (a is Pawn pawn1 && pawn1.IsSlave(out slaveComp1))
            {
                if (b is Pawn pawn2 && pawn2.IsSlave(out slaveComp2))
                {
                    __result = slaveComp1.slaverFaction.HostileTo(slaveComp2.slaverFaction);
                }
                else
                {
                    __result = b.HostileTo(slaveComp1.slaverFaction);
                }
                Log.Message($"a: {a}, b: {b}, result: {__result}");
            }
            if (b is Pawn pawn3 && pawn3.IsSlave(out slaveComp2))
            {
                if (a is Pawn pawn4 && pawn4.IsSlave(out slaveComp1))
                {
                    __result = slaveComp2.slaverFaction.HostileTo(slaveComp1.slaverFaction);
                }
                else
                {
                    __result = a.HostileTo(slaveComp2.slaverFaction);
                }
                Log.Message($"a: {a}, b: {b}, result: {__result}");
            }
        }
    }

    [HarmonyPatch(typeof(Building_Door), "PawnCanOpen")]
    public static class PawnCanOpen_Patch
    {
        public static void Postfix(Building_Door __instance, ref bool __result, Pawn p)
        {
            if (!__result && p.IsSlave(out var slaveComp) && __instance.Faction == slaveComp.slaverFaction)
            {
                __result = true;
            }
        }
    }
}
