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

namespace FriendsByForce
{
	public class Designator_ZoneAdd_SlaveLabor : Designator_ZoneAdd
	{
		protected override string NewZoneLabel => "FBF.SlaveLaborZone".Translate();

		public Designator_ZoneAdd_SlaveLabor()
		{
			zoneTypeToPlace = typeof(Zone_SlaveLabor);
			defaultLabel = "SlaveLaborZone".Translate();
			defaultDesc = "DesignatorSlaveLaborZoneDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Growing");
			hotKey = KeyBindingDefOf.Misc2;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!base.CanDesignateCell(c).Accepted)
			{
				return false;
			}
			return true;
		}

		protected override Zone MakeNewZone()
		{
			return new Zone_SlaveLabor(Find.CurrentMap.zoneManager);
		}
	}
}