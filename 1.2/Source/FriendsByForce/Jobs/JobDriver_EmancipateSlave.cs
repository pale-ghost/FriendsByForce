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
    public class JobDriver_EmancipateSlave : JobDriver
    {
        public Pawn Slave => job.targetA.Pawn;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !Slave.IsSlave(out var slaveComp) || !slaveComp.markedForEmancipation);
            yield return GotoSlave(pawn, Slave);
            yield return EmancipateSlave(pawn, Slave);
        }

        private Toil EmancipateSlave(Pawn pawn, Pawn slave)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                var slaveComp = Utils.GetCachedSlaveComp(slave);
                slaveComp.Emancipate(pawn);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
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
    }
}