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
	public class Thought_WasBeaten : Thought_Memory
    {
        public int beatingCount;
        public override float MoodOffset()
        {
            return -beatingCount;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref beatingCount, "beatingCount");
        }
    }
}
