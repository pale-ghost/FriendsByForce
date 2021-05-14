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
    [DefOf]
    public static class FBF_DefOf
    {
        public static PrisonerInteractionModeDef FBF_EnslaveInteractionMode;
        public static ThingDef FBF_RopeCollar;
        public static JobDef FBF_EnslavePrisoner;
        public static TraitDef Wimp;
        public static TraitDef Masochist;
        public static JobDef FBF_BeatSlave;
        public static JobDef FBF_StandAndAcceptBeating;
        public static ThoughtDef FBF_WasBeaten;
    }
}
