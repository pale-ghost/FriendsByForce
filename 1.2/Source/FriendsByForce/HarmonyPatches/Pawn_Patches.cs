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
                if (pawn.IsSlave(out CompEnslavement comp) && __result.targetA.Thing != null && __result.targetA.Thing.IsSlaveCollar() && pawn.HasSlaveCollar())
                {
                    __result = null;
                    if (__state && comp.previousFaction != pawn.Faction)
                    {
                        pawn.SetFaction(comp.previousFaction);
                    }
                }
                else if (!pawn.IsSlave() && pawn.IsColonist && __result.targetA.Thing != null && __result.targetA.Thing.IsSlaveCollar())
                {
                    __result = null;
                }
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
}
