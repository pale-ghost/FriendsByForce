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
    public class CompProperties_Enslavement : CompProperties
    {
        public CompProperties_Enslavement()
        {
            this.compClass = typeof(CompEnslavement);
        }
    }

    public class CompEnslavement : ThingComp
    {
        public Pawn Pawn => this.parent as Pawn;
        public bool isSlave;
        public Faction previousFaction;
        public Faction slaverFaction;
        public void EnslaveWith(Apparel collar, Pawn slaver)
        {
            previousFaction = Pawn.Faction;
            slaverFaction = slaver.Faction;
            Enslave();
            GiveSlaveCollar(collar);
        }
        private void Enslave()
        {
            isSlave = true;
            Pawn.guest.isPrisonerInt = false;
            if (Pawn.workSettings is null)
            {
                Pawn.workSettings = new Pawn_WorkSettings(Pawn);
            }
            Pawn.workSettings.EnableAndInitialize();
        }
        private void GiveSlaveCollar(Apparel collar)
        {
            Pawn.apparel.Wear(collar, true);
            if (Pawn.outfits == null)
                Pawn.outfits = new Pawn_OutfitTracker();
            Pawn.outfits.forcedHandler.SetForced(collar, true);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isSlave, "isSlave");
            Scribe_References.Look(ref previousFaction, "previousFaction");
            Scribe_References.Look(ref slaverFaction, "slaverFaction");
        }
    }
}
