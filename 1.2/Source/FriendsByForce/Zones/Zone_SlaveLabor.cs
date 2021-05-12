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
	public class Zone_SlaveLabor : Zone
	{
		public override bool IsMultiselectable => true;

		protected override Color NextZoneColor => Color.red;

		public Zone_SlaveLabor()
		{
		}

		public Zone_SlaveLabor(ZoneManager zoneManager) : base("FBF.SlaveLaborZone".Translate(), zoneManager)
		{

		}

		public override void ExposeData()
		{
			base.ExposeData();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
		}

		public override IEnumerable<Gizmo> GetZoneAddGizmos()
		{
			yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_SlaveLabor_Expand>();
		}
	}
}