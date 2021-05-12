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
	public class Area_SlaveLabor : Area
	{
		public Area_SlaveLabor()
		{
		}

		public Area_SlaveLabor(AreaManager areaManager) : base(areaManager)
		{
		}

		public override string Label => "FBF.SlaveLaborZone".Translate();

		public override Color Color => Color.red;

		public override int ListPriority => 8000;

		public override string GetUniqueLoadID()
		{
			return "Area_SlaveLabor" + ID + "_Labor";
		}
	}
}