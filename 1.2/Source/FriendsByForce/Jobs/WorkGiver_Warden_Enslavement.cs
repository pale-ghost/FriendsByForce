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
	public class WorkGiver_Warden_Enslavement : WorkGiver_Warden
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
		public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!ShouldTakeCareOfPrisoner_NewTemp(pawn, t))
			{
				return null;
			}
			Pawn prisoner = (Pawn)t;
			if (prisoner.guest.interactionMode != FBF_DefOf.FBF_EnslaveInteractionMode || pawn.IsSlave() || !pawn.CanReserve(prisoner, 1, -1, null, false))
			{
				return null;
			}
			var slaveCollar = pawn.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Apparel))
				.Find(x => x.IsSlaveCollar() && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.ClosestTouch, Danger.Deadly));

			return slaveCollar != null ? JobMaker.MakeJob(FBF_DefOf.FBF_EnslavePrisoner, prisoner, slaveCollar) : null;
		}
	}
}
