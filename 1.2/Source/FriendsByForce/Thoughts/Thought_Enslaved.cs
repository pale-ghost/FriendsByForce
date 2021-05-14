using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FriendsByForce
{
	public class Thought_Enslaved : Thought_Situational
    {
        public override float MoodOffset()
        {
            var slaveComp = pawn.GetCachedSlaveComp();
            if (slaveComp.willpower > 0)
            {
                Log.Message("slaveComp.willpower: " + slaveComp.willpower + " - " + -(slaveComp.willpower * 20f));
                return -(slaveComp.willpower * 20f);
            }
            return -1f;
        }
    }
}
