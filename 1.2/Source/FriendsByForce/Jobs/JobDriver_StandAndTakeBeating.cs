using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FriendsByForce
{
    public class JobDriver_StandAndTakeBeating : JobDriver
    {
        protected Pawn Attacker => job.targetA.Pawn;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate
            {
                pawn.pather.StopDead();
            };
            toil.tickAction = delegate
            {
                if (Attacker.jobs.curDriver is JobDriver_BeatSlave attackerJobDriver)
                {
                    
                }
                else if (pawn.IsHashIntervalTick(120))
                {
                    EndJobWith(JobCondition.Succeeded);
                }
            };
            yield return toil;
        }
    }
}