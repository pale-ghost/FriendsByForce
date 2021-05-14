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
    [StaticConstructorOnStartup]
    public static class Utils
    {
        static Utils()
        {
            foreach (var human in DefDatabase<ThingDef>.AllDefs.Where(x => x.race?.Humanlike ?? false))
            {
                human.comps.Add(new CompProperties_Enslavement());
            }
            DefDatabase<DesignationCategoryDef>.GetNamed("Zone").AllResolvedDesignators.Add(new Designator_AreaSlaveLaborExpand());
            DefDatabase<DesignationCategoryDef>.GetNamed("Zone").AllResolvedDesignators.Add(new Designator_AreaSlaveLaborClear());
        }
        public static bool IsSlaveCollar(this Thing thing)
        {
            return thing.def.IsSlaveCollar();
        }
        public static bool IsSlaveCollar(this ThingDef thingDef)
        {
            return FBF_DefOf.FBF_RopeCollar == thingDef;
        }

        private static Dictionary<Pawn, CompEnslavement> cachedComps = new Dictionary<Pawn, CompEnslavement>();
        public static CompEnslavement GetCachedSlaveComp(this Pawn pawn)
        {
            if (!cachedComps.TryGetValue(pawn, out var comp) || comp is null)
            {
                comp = pawn.TryGetComp<CompEnslavement>();
                cachedComps[pawn] = comp;
            }
            return comp;
        }

        public static bool IsSlave(this Pawn pawn)
        {
            return IsSlave(pawn, out CompEnslavement slaveComp);
        }
        public static bool IsSlave(this Pawn pawn, out CompEnslavement slaveComp)
        {
            slaveComp = pawn.GetCachedSlaveComp();
            if (slaveComp != null)
            {
                return slaveComp.isSlave;
            }
            return false;
        }

        public static bool HasSlaveCollar(this Pawn pawn, out Apparel slaveCollar)
        {
            slaveCollar = null;
            if (pawn.apparel == null)
            {
                return false;
            }
            foreach (Apparel item in pawn.apparel.WornApparel)
            {
                if (IsSlaveCollar(item))
                {
                    slaveCollar = item;
                    return true;
                }
            }
            return false;
        }

        public static bool CanUseIt(this Pawn pawn, Thing thing)
        {
            if (pawn.IsSlave(out var slaveComp))
            {
                var slaveLaborArea = pawn.Map?.areaManager?.Get<Area_SlaveLabor>();
                if (slaveLaborArea != null)
                {
                    return slaveLaborArea[thing.PositionHeld];
                }
            }
            return true;
        }
    }
}
