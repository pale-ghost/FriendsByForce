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
	public class Designator_ZoneAdd_SlaveLabor_Expand : Designator_ZoneAdd_SlaveLabor
	{
		public Designator_ZoneAdd_SlaveLabor_Expand()
		{
			defaultLabel = "DesignatorZoneExpand".Translate();
			hotKey = KeyBindingDefOf.Misc6;
		}
	}
}