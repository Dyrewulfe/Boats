﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Harmony;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimShips.Build;
using RimShips.Defs;
using RimShips.AI;
using UnityEngine;
using UnityEngine.AI;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimShips.Jobs
{
    public class JobGiver_GotoTravelDestinationOcean : ThinkNode_JobGiver
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GotoTravelDestinationOcean job = (JobGiver_GotoTravelDestinationOcean)base.DeepCopy(resolve);
            job.locomotionUrgency = this.locomotionUrgency;
            job.maxDanger = this.maxDanger;
            job.jobMaxDuration = this.jobMaxDuration;
            job.exactCell = this.exactCell;
            return job;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
            if (pawn.mindState.nextMoveOrderIsWait && !this.exactCell)
            {
                return new Job(JobDefOf_Ships.IdleShip)
                {
                    expiryInterval = this.WaitTicks.RandomInRange
                };
            }
            IntVec3 cell = pawn.mindState.duty.focus.Cell;
            if (!ShipReachabilityUtility.CanReachShip(pawn, cell, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(pawn, this.maxDanger), false, TraverseMode.ByPawn))
                return null;
            if (this.exactCell && pawn.Position == cell)
                return null;
            IntVec3 c = cell;
            
            return new Job(JobDefOf.Goto, c)
            {
                locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, this.locomotionUrgency),
                expiryInterval = this.jobMaxDuration
            };
        }

        private LocomotionUrgency locomotionUrgency = LocomotionUrgency.Jog;

        private Danger maxDanger = Danger.Some;

        private int jobMaxDuration = 999999;

        private bool exactCell;

        private IntRange WaitTicks = new IntRange(30, 80);
    }
}