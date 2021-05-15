using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FriendsByForce
{
	public class JobGiver_TryToEscape : JobGiver_ExitMapBest
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			return Utils.EscapeJob(pawn, true, false, true, true, true);
		}
	}
}