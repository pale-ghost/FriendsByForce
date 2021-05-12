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
    public abstract class Designator_AreaSlaveLabor : Designator_Area
    {
        private static readonly List<IntVec3> JustRemovedCells = new List<IntVec3>();

        private static readonly List<IntVec3> JustAddedCells = new List<IntVec3>();

        private static readonly List<Room> RequestedRooms = new List<Room>();

        private readonly DesignateMode mode;
        public Designator_AreaSlaveLabor(DesignateMode mode)
        {
            this.mode = mode;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
            defaultLabel = "FBF.SlaveLaborZone".Translate();
        }

        public override int DraggableDimensions => 2;

        public override bool DragDrawMeasurements => true;

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            return AcceptanceReport.WasRejected;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
                return false;
            var flag = Map.areaManager.Get<Area_SlaveLabor>()[c];
            if (mode == DesignateMode.Add)
                return !flag;
            return flag;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (mode == DesignateMode.Add)
            {
                Map.areaManager.Get<Area_SlaveLabor>()[c] = true;
                JustAddedCells.Add(c);
            }
            else if (mode == DesignateMode.Remove)
            {
                Map.areaManager.Get<Area_SlaveLabor>()[c] = false;
                JustRemovedCells.Add(c);
            }
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            if (mode == DesignateMode.Add)
            {
                for (var i = 0; i < JustAddedCells.Count; i++)
                    Map.areaManager.Get<Area_SlaveLabor>()[JustAddedCells[i]] = true;
                JustAddedCells.Clear();
            }
            else if (mode == DesignateMode.Remove)
            {
                for (var j = 0; j < JustRemovedCells.Count; j++)
                    Map.areaManager.Get<Area_SlaveLabor>()[JustRemovedCells[j]] = false;
                JustRemovedCells.Clear();
                RequestedRooms.Clear();
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            if (Map.areaManager.Get<Area_SlaveLabor>() == null)
                Map.areaManager.AllAreas.Add(new Area_SlaveLabor(Map.areaManager));
            Map.areaManager.Get<Area_SlaveLabor>().MarkForDraw();
        }
    }
    public class Designator_AreaSlaveLaborClear : Designator_AreaSlaveLabor
    {
        public Designator_AreaSlaveLaborClear() : base(DesignateMode.Remove)
        {
            defaultLabel = "FBF.ClearSlaveLaborArea".Translate();
            defaultDesc = "FBF.SlaveLaborAreaDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("FBF.SlaveLaborAreaClear", true);
            soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            soundDragChanged = null;
            soundSucceeded = SoundDefOf.Designate_ZoneDelete;
        }
    }

    public class Designator_AreaSlaveLaborExpand : Designator_AreaSlaveLabor
    {
        public Designator_AreaSlaveLaborExpand() : base(DesignateMode.Add)
        {
            defaultLabel = "FBF.ExpandSlaveLaborArea".Translate();
            defaultDesc = "FBF.SlaveLaborAreaDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("FBF.SlaveLaborAreaExpand", true);
            soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
            soundDragChanged = null;
            soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        }
    }
}