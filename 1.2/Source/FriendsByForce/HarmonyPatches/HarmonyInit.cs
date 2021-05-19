using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FriendsByForce
{
	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{
		static HarmonyInit()
		{
			Harmony harmony = new Harmony("Ghost.FriendsByForce");
			harmony.PatchAll();
		}
	}


	[HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
	public class StartJobPatch
	{
	    private static void Postfix(Pawn_JobTracker __instance, Pawn ___pawn, Job newJob, JobTag? tag)
	    {
	        if (___pawn.IsSlave())
	        {
				Log.Message(___pawn + " is starting " + newJob);
			}
		}
	}
	
	
	[HarmonyPatch(typeof(Pawn_JobTracker), "EndCurrentJob")]
	public class EndCurrentJobPatch
	{
	    private static void Prefix(Pawn_JobTracker __instance, Pawn ___pawn, JobCondition condition, ref bool startNewJob, bool canReturnToPool = true)
	    {
			if (___pawn.IsSlave())
			{
				Log.Message(___pawn + " is ending " + ___pawn.CurJob);
			}
		}
	}
	
	[HarmonyPatch(typeof(ThinkNode_JobGiver), "TryIssueJobPackage")]
	public class TryIssueJobPackage
	{
		private static void Postfix(ThinkNode_JobGiver __instance, ThinkResult __result, Pawn pawn, JobIssueParams jobParams)
		{
			if (pawn.IsSlave() && __result.Job != null)
			{
				Log.Message(pawn + " gets " + __result.Job + " from " + __instance);
			}
		}
	}
	[HarmonyPatch(typeof(Pawn), "GetInspectString")]
	public class GetInspectString_Patch
	{
		private static void Postfix(Pawn __instance, ref string __result)
		{
			__result += "\n";
			__result += "job: " + __instance.CurJob + "\n";
			__result += "driver: " + __instance.jobs.curDriver + "\n";
			__result += "duty: " + __instance.mindState.duty + "\n";
			__result += "lord: " + __instance.GetLord()?.LordJob + "\n";
			__result += "Faction: " + __instance.Faction + "\n";
			__result += "HostFaction: " + __instance.HostFaction + "\n";
			__result += "IsColonist: " + __instance.IsColonist + "\n";
			__result += "IsColonistPlayerControlled: " + __instance.IsColonistPlayerControlled + "\n";
			__result += "IsFreeColonist: " + __instance.IsFreeColonist;
		}
	}
}
