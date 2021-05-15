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
using Verse.AI;

namespace FriendsByForce
{
    public class ThinkNode_Conditional_CanDoEscapeAttempt : ThinkNode_Conditional
    {
        public float mtbHours;
        protected override bool Satisfied(Pawn pawn)
        {
            var slaveComp = Utils.GetCachedSlaveComp(pawn);
            if (slaveComp != null && Find.TickManager.TicksGame >= slaveComp.lastEscapeAttemptTick + (GenDate.TicksPerHour * mtbHours))
            {
                Log.Message(" - Satisfied - var moodLevel = pawn.needs.mood.CurLevelPercentage; - 3", true);
                var moodLevel = pawn.needs.mood.CurLevelPercentage;
                Log.Message(" - Satisfied - if (moodLevel >= 0.97f) - 4", true);
                if (moodLevel >= 0.97f)
                {
                    Log.Message(" - Satisfied - return false; - 5", true);
                    return false;
                }
                Log.Message(" - Satisfied - if (slaveComp.willpower <= 0) - 6", true);
                if (slaveComp.willpower <= 0)
                {
                    Log.Message(" - Satisfied - if (moodLevel <= pawn.mindState.mentalBreaker.BreakThresholdExtreme) - 7", true);
                    if (moodLevel <= pawn.mindState.mentalBreaker.BreakThresholdExtreme)
                    {
                        Log.Message(" - Satisfied - return Rand.Chance(0.05f); - 8", true);
                        return Rand.Chance(0.05f);
                    }
                }
                else
                {
                    Log.Message(" - Satisfied - return Rand.Chance(0.5f); - 9", true);
                    return Rand.Chance(0.5f);
                }
            }
            return false;
        }
    }
}