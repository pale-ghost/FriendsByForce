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
    public class JobDriver_EnslavePrisoner : JobDriver
    {
        const int enslaveDuration = 300;
        private Pawn Victim => job.GetTarget(TargetIndex.A).Thing as Pawn;
        private Apparel SlaveCollar => job.GetTarget(TargetIndex.B).Thing as Apparel;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Victim, job, 1, -1, null) && pawn.Reserve(SlaveCollar, job, 1, -1, null);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnForbidden(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    pawn.jobs.curJob.count = 1;
                }
            };
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, enslaveDuration, true);
            yield return new Toil
            {
                initAction = delegate
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.PositionHeld, ThingPlaceMode.Direct, out Thing droppedThing, null);
                    if (droppedThing is Apparel slaveCollar)
                    {
                        bool success = true;
                        var slaveComp = Utils.GetCachedSlaveComp(Victim);

                        if ((!Victim.jobs?.curDriver?.asleep ?? false) && (!Victim.story?.traits?.HasTrait(FBF_DefOf.Wimp) ?? false) && !Victim.InMentalState && !Victim.Downed)
                        {
                            var victimMeleeSkill = Victim.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0;
                            var wardenMeleeSkill = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0;
                            if (victimMeleeSkill > wardenMeleeSkill)
                            {
                                var chance = (victimMeleeSkill / 20f) - (wardenMeleeSkill / 20f);
                                if (Rand.Chance(chance))
                                {
                                    success = false;
                                }
                            }
                        }
                        if (success)
                        {
                            slaveComp.EnslaveWith(slaveCollar, pawn);
                            Messages.Message("FBF.EnslavedPrisoner".Translate(pawn.Named("PAWN"), Victim.Named("SLAVE")), MessageTypeDefOf.PositiveEvent);
                            AddEndCondition(() => JobCondition.Succeeded);
                        }
                        else
                        {
                            Messages.Message("FBF.FailedToEnslave".Translate(pawn.Named("PAWN"), Victim.Named("SLAVE")), MessageTypeDefOf.ThreatSmall);
                            Victim.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.Berserk);
                            AddEndCondition(() => JobCondition.Incompletable);
                        }
                    }
                    else
                        AddEndCondition(() => JobCondition.Incompletable);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}