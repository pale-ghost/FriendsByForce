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
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!isInitialized)
            {
                isInitialized = true;
                willpower = GetInitialWillpower();
                maxWillpower = willpower;
            }
        }

        private float GetInitialWillpower()
        {
            var pawn = Pawn;
            if (pawn.story.traits != null)
            {
                var nerves = pawn.story.traits.GetTrait(TraitDefOf.Nerves);
                if (nerves != null && (nerves.Degree == 2 || nerves.Degree == 1 || nerves.Degree == -2))
                {
                    return 2f;
                }
                if (pawn.story.traits.HasTrait(TraitDefOf.Tough))
                {
                    return 2f;
                }
                if (pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                {
                    return 2f;
                }
                if (pawn.story.traits.HasTrait(TraitDefOf.Abrasive))
                {
                    return 1.5f;
                }
                var industriousness = pawn.story.traits.GetTrait(TraitDefOf.Industriousness);
                if (industriousness != null && (industriousness.Degree == -1 || industriousness.Degree == -2))
                {
                    return 1.25f;
                }
                if (pawn.story.traits.HasTrait(FBF_DefOf.Wimp))
                {
                    return 0.5f;
                }
                if (nerves != null && nerves.Degree == -1)
                {
                    return 0.5f;
                }
                if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic))
                {
                    return 0.25f;
                }
                if (pawn.story.traits.HasTrait(FBF_DefOf.Masochist))
                {
                    return 0.05f;
                }
            }
            return 1f;
        }
        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame > nextWillpowerTick)
            {
                nextWillpowerTick = Find.TickManager.TicksGame + 60000;
                willpower = Mathf.Max(willpower - (0.03f * Mathf.Max(Pawn.needs.mood.CurLevelPercentage, 0.01f)), 0f);
            }
        }
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
            nextWillpowerTick = Find.TickManager.TicksGame + 60000;
        }

        public void EscapeFromSlavers()
        {
            isSlave = false;
            Pawn.guest.everParticipatedInPrisonBreak = true;
            Pawn.guest.lastPrisonBreakTicks = Find.TickManager.TicksGame;
        }
        private void GiveSlaveCollar(Apparel collar)
        {
            Pawn.apparel.Wear(collar, true);
            if (Pawn.outfits == null)
                Pawn.outfits = new Pawn_OutfitTracker();
            Pawn.outfits.forcedHandler.SetForced(collar, true);
        }

        public const int BeatingCooldown = 60000;
        public bool CanBeBeaten => markedForBeating && Find.TickManager.TicksGame > lastBeatenTick + BeatingCooldown && Pawn.health.summaryHealth.SummaryHealthPercent > LowHealthStopBeating;

        public const float LowHealthStopBeating = 0.25f;

        public void DoBeatingOutcome()
        {
            this.willpower -= 0.05f;
            var moodOffset = 1;
            var wasBeatenThought = Pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(FBF_DefOf.FBF_WasBeaten) as Thought_WasBeaten;
            if (wasBeatenThought != null)
            {
                moodOffset += wasBeatenThought.beatingCount;
                Pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(FBF_DefOf.FBF_WasBeaten);
            }
            var thought = (Thought_WasBeaten)ThoughtMaker.MakeThought(FBF_DefOf.FBF_WasBeaten);
            thought.beatingCount = moodOffset;
            Pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.CompFloatMenuOptions(selPawn))
            {
                yield return opt;
            }
            if (selPawn.Faction == Faction.OfPlayer && this.isSlave && markedForBeating)
            {
                if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
                {
                    yield return new FloatMenuOption("FBF.Beat".Translate() + " (" + "NoPath".Translate() + ")", null);
                }
                else if (!selPawn.CanReserve(parent))
                {
                    yield return new FloatMenuOption("FBF.Beat".Translate() + " (" + "Reserved".Translate() + ")", null);
                }
                else if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    yield return new FloatMenuOption("FBF.Beat".Translate() + " (" + "Incapable".Translate() + ")", null);
                }
                else if (Find.TickManager.TicksGame > lastBeatenTick + BeatingCooldown)
                {
                    yield return new FloatMenuOption("FBF.Beat".Translate() + " (" + "FBF.TooRecentlyBeaten".Translate() + ")", null);
                }
                else if (Pawn.health.summaryHealth.SummaryHealthPercent < LowHealthStopBeating)
                {
                    yield return new FloatMenuOption("FBF.Beat".Translate() + " (" + "FBF.TooInjured".Translate() + ")", null);
                }
                else
                {
                    yield return new FloatMenuOption("FBF.Beat".Translate(), delegate
                    {
                        Job beatSlave = JobMaker.MakeJob(FBF_DefOf.FBF_BeatSlave, Pawn);
                        beatSlave.maxNumMeleeAttacks = Rand.RangeInclusive(3, 5);
                        beatSlave.locomotionUrgency = LocomotionUrgency.Jog;
                        selPawn.jobs.TryTakeOrderedJob(beatSlave);
                    });
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isSlave, "isSlave");
            Scribe_References.Look(ref previousFaction, "previousFaction");
            Scribe_References.Look(ref slaverFaction, "slaverFaction");
            Scribe_Values.Look(ref willpower, "willpower");
            Scribe_Values.Look(ref maxWillpower, "maxWillpower");
            Scribe_Values.Look(ref isInitialized, "isInitialized");
            Scribe_Values.Look(ref nextWillpowerTick, "nextTick");
            Scribe_Values.Look(ref markedForBeating, "markedForBeating");
            Scribe_Values.Look(ref lastBeatenTick, "lastBeatenTick");
            Scribe_Values.Look(ref lastEscapeAttemptTick, "lastEscapeAttemptTick");
        }

        public bool isSlave;
        public Faction previousFaction;
        public Faction slaverFaction;
        public float willpower;
        private float maxWillpower;
        private bool isInitialized;
        private int nextWillpowerTick;
        public bool markedForBeating;
        public int lastBeatenTick;
        public int lastEscapeAttemptTick;
    }
}
