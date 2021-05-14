using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FriendsByForce
{
	public class ThoughtWorker_Enslaved : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.IsSlave(out var slaveComp))
            {
				if (slaveComp.willpower > 0)
				{
					return ThoughtState.ActiveAtStage(0);
				}
				return ThoughtState.ActiveAtStage(1);
			}
			return ThoughtState.Inactive;
		}
	}
}
