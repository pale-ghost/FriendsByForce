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
    public class JobDriver_BeatSlave : JobDriver
    {
        private int numMeleeAttacksMade;
        public Pawn Victim => job.targetA.Pawn;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Victim, job);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Message(" - MakeNewToils - this.FailOn(() => !Victim.IsSlave()); - 1", true);
            this.FailOn(() => !Victim.IsSlave(out var slaveComp) || !slaveComp.markedForBeating);
            yield return GotoSlave(pawn, Victim);
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            {
                Log.Message(" - MakeNewToils - Thing thing = job.GetTarget(TargetIndex.A).Thing; - 4", true);
                Thing thing = job.GetTarget(TargetIndex.A).Thing;
                Log.Message(" - MakeNewToils - Pawn p; - 5", true);
                Pawn p;
                Log.Message(" - MakeNewToils - if (job.reactingToMeleeThreat && (p = (thing as Pawn)) != null && !p.Awake()) - 6", true);
                if (job.reactingToMeleeThreat && (p = (thing as Pawn)) != null && !p.Awake())
                {
                    Log.Message(" - MakeNewToils - EndJobWith(JobCondition.InterruptForced); - 7", true);
                    EndJobWith(JobCondition.InterruptForced);
                }
                else if (pawn.meleeVerbs.TryMeleeAttack(thing, job.verbToUse) && pawn.CurJob != null && pawn.jobs.curDriver == this)
                {
                    var slaveComp = Utils.GetCachedSlaveComp(Victim);
                    if (Victim.HasSlaveCollar(out var slaveCollar))
                    {
                        slaveCollar.Damage(0.1f);
                        if (slaveCollar.DestroyedOrNull())
                        {
                            var escapeJob = Utils.EscapeJob(Victim, false, false, true, true, true);
                            if (escapeJob != null)
                            {
                                Victim.jobs.TryTakeOrderedJob(escapeJob);
                                EndJobWith(JobCondition.InterruptForced);
                            }
                        }
                    }
                    if (Victim.health.summaryHealth.SummaryHealthPercent < CompEnslavement.LowHealthStopBeating)
                    {
                        slaveComp.DoBeatingOutcome();
                        EndJobWith(JobCondition.Succeeded);
                    }
                    Log.Message(" - MakeNewToils - if (Victim.CurJobDef != FBF_DefOf.FBF_StandAndAcceptBeating) - 9", true);
                    if (Victim.CurJobDef != FBF_DefOf.FBF_StandAndAcceptBeating)
                    {
                        Log.Message(" - MakeNewToils - Job job = JobMaker.MakeJob(FBF_DefOf.FBF_StandAndAcceptBeating, pawn); - 10", true);
                        Job job = JobMaker.MakeJob(FBF_DefOf.FBF_StandAndAcceptBeating, pawn);
                        Log.Message(" - MakeNewToils - Victim.jobs.TryTakeOrderedJob(job); - 11", true);
                        Victim.jobs.TryTakeOrderedJob(job);
                    }
                    slaveComp.lastBeatenTick = Find.TickManager.TicksGame;
                    Log.Message(" - MakeNewToils - numMeleeAttacksMade++; - 14", true);
                    numMeleeAttacksMade++;
                    Log.Message(" - MakeNewToils - if (numMeleeAttacksMade >= job.maxNumMeleeAttacks) - 15", true);
                    if (numMeleeAttacksMade >= job.maxNumMeleeAttacks)
                    {
                        slaveComp.DoBeatingOutcome();
                        Log.Message(" - MakeNewToils - EndJobWith(JobCondition.Succeeded); - 16", true);
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
            }).FailOnDespawnedOrNull(TargetIndex.A);
        }


        private Toil GotoSlave(Pawn pawn, Pawn slave)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                pawn.pather.StartPath(slave, PathEndMode.Touch);
            };
            toil.AddFailCondition(delegate
            {
                if (slave.DestroyedOrNull())
                {
                    return true;
                }
                if (!slave.IsSlave())
                {
                    return true;
                }
                return false;
            });
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref numMeleeAttacksMade, "numMeleeAttacksMade");
        }
    }
}