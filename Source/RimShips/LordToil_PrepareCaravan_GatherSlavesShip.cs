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
using UnityEngine;
using UnityEngine.AI;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimShips.AI;
using RimShips.Defs;
using RimShips.Build;
using RimShips.Jobs;
using RimShips.UI;

namespace RimShips.Lords
{
    public class LordToil_PrepareCaravan_GatherSlavesShip : LordToil
    {
        public LordToil_PrepareCaravan_GatherSlavesShip(IntVec3 meetingPoint)
        {
            this.meetingPoint = meetingPoint;
        }

        public override float? CustomWakeThreshold
        {
            get
            {
                return new float?(0.5f);
            }
        }

        public override bool AllowRestingInBed
        {
            get
            {
                return false;
            }
        }

        public override void UpdateAllDuties()
        {
            foreach(Pawn p in this.lord.ownedPawns)
            {
                if (ShipHarmony.IsShip(p))
                {
                    p.mindState.duty = new PawnDuty(DutyDefOf_Ships.PrepareCaravan_WaitShip);
                }
                else if(!p.RaceProps.Animal && (!ShipHarmony.IsShip(p)))
                {
                    p.mindState.duty = new PawnDuty(DutyDefOf_Ships.PrepareCaravan_SendSlavesToShip, this.meetingPoint, -1f);
                    p.mindState.duty.pawnsToGather = PawnsToGather.Slaves;
                }
                else
                {
                    p.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, this.meetingPoint, -1f);
                }
            }
        }

        public override void LordToilTick()
        {
            if(Find.TickManager.TicksGame % 100 == 0)
            {
                Lord lord = this.lord;
                List<Pawn> pawns = this.lord.ownedPawns.Where(x => !ShipHarmony.IsShip(x)).ToList();

                IntVec3 intVec = this.meetingPoint;
                string memo = "AllSlavesGathered";
                Predicate<Pawn> shouldCheckIfArrived = (Pawn x) => !x.IsColonist && !x.RaceProps.Animal;

                GatherAnimalsAndSlavesForShipsUtility.CheckArrived(lord, pawns, intVec, memo, shouldCheckIfArrived, false, null);
            }
        }

        private IntVec3 meetingPoint;
    }
}
