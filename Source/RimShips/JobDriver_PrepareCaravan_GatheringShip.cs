﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Harmony;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimShips.AI;
using RimShips.UI;
using RimShips.Jobs;
using RimShips.Lords;
using UnityEngine;
using UnityEngine.AI;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimShips.Jobs
{
    public class JobDriver_PrepareCaravan_GatheringShip : JobDriver
    {
        public Thing ToHaul
        {
            get
            {
                return this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public Pawn Carrier
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        private List<TransferableOneWay> Transferables
        {
            get
            {
                return ((LordJob_FormAndSendCaravanShip)this.job.lord.LordJob).transferables;
            }
        }

        private TransferableOneWay Transferable
        {
            get
            {
                TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(this.ToHaul, this.Transferables,
                    TransferAsOneMode.PodsOrCaravanPacking);
                if(!(transferableOneWay is null))
                {
                    return transferableOneWay;
                }
                throw new InvalidOperationException("Could not find any matching transferable.");
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.ToHaul;
            Job job = this.job;
            return pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !base.Map.lordManager.lords.Contains(this.job.lord));
            Toil reserve = Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null).FailOnDespawnedOrNull(TargetIndex.A);
            yield return reserve;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return this.DetermineNumToHaul();
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false);
            yield return this.AddCarriedThingToTransferables();
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserve, TargetIndex.A, TargetIndex.None, true, (Thing x) =>
                this.Transferable.things.Contains(x));
            Toil findCarrier = this.FindCarrier();
            yield return findCarrier;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).JumpIf(() => !JobDriver_PrepareCaravan_GatheringShip.IsUsableCarrier(this.Carrier, this.pawn),
                findCarrier);
            yield return Toils_General.Wait(25, TargetIndex.None).JumpIf(() => !JobDriver_PrepareCaravan_GatheringShip.IsUsableCarrier(this.Carrier, this.pawn),
                findCarrier).WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            yield return this.PlaceTargetInCarrierInventory();
            yield break;
        }

        private Toil DetermineNumToHaul()
        {
            return new Toil
            {
                initAction = delegate ()
                {
                    int num = GatherItemsForShipCaravanUtility.CountLeftToTransfer(this.pawn, this.Transferable, this.job.lord);
                    if (!(this.pawn.carryTracker.CarriedThing is null))
                    {
                        num -= this.pawn.carryTracker.CarriedThing.stackCount;
                    }
                    if (num <= 0)
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    }
                    else
                    {
                        this.job.count = num;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant,
                atomicWithPrevious = true
            };
        }

        private Toil AddCarriedThingToTransferables()
        {
            return new Toil
            {
                initAction = delegate ()
                {
                    TransferableOneWay transferable = this.Transferable;
                    if (!transferable.things.Contains(this.pawn.carryTracker.CarriedThing))
                    {
                        transferable.things.Add(this.pawn.carryTracker.CarriedThing);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant,
                atomicWithPrevious = true
            };
        }

        private Toil FindCarrier()
        {
            return new Toil
            {
                initAction = delegate ()
                {
                    Pawn pawn = this.FindBestCarrierShips();
                    if (pawn is null)
                    {
                        bool flag = this.pawn.GetLord() == this.job.lord;
                        if (flag && !MassUtility.IsOverEncumbered(this.pawn))
                        {
                            pawn = this.pawn;
                        }
                        else
                        {
                            if(pawn is null)
                            {
                                if (flag)
                                {
                                    pawn = this.pawn;
                                }
                                else
                                {
                                    IEnumerable<Pawn> source = from x in this.job.lord.ownedPawns
                                                               where JobDriver_PrepareCaravan_GatheringShip.IsUsableCarrier(x, this.pawn)
                                                               select x;
                                    if(!source.Any<Pawn>())
                                    {
                                        base.EndJobWith(JobCondition.Incompletable);
                                        return;
                                    }
                                    pawn = source.RandomElement<Pawn>();
                                }
                            }
                        }
                    }
                    this.job.SetTarget(TargetIndex.B, pawn);
                }
            };
        }

        private Toil PlaceTargetInCarrierInventory()
        {
            return new Toil
            {
                initAction = delegate ()
                {
                    Pawn_CarryTracker carryTracker = this.pawn.carryTracker;
                    Thing carriedThing = carryTracker.CarriedThing;
                    this.Transferable.AdjustTo(Mathf.Max(this.Transferable.CountToTransfer - carriedThing.stackCount, 0));
                    carryTracker.innerContainer.TryTransferToContainer(carriedThing, this.Carrier.inventory.innerContainer,
                        carriedThing.stackCount, true);
                }
            };
        }

        public static bool IsUsableCarrier(Pawn ship, Pawn forPawn)
        {
            return ship.IsFormingCaravan() && (!ship.DestroyedOrNull() && ship.Spawned) && ship.Faction == forPawn.Faction 
                && !ship.IsBurning() && ship.GetComp<CompShips>().movementStatus != ShipMovementStatus.Offline
                && !MassUtility.IsOverEncumbered(ship);
        }

        private float GetCarrierScore(Pawn pawn)
        {
            return (1f - MassUtility.EncumbrancePercent(pawn)) - (pawn.Position - this.pawn.Position).LengthHorizontal / 10f * 0.2f;
        }

        private Pawn FindBestCarrierShips()
        {
            Lord lord = this.job.lord;
            Pawn pawn = null;
            float num = 0f;
            if(!(lord is null))
            {
                foreach(Pawn p in lord.ownedPawns)
                {
                    if(p != this.pawn && !(p.GetComp<CompShips>() is null) )
                    {
                        if(JobDriver_PrepareCaravan_GatheringShip.IsUsableCarrier(p,this.pawn))
                        {
                            float carrierScore = this.GetCarrierScore(p);
                            if(pawn is null || carrierScore > num)
                            {
                                pawn = p;
                                num = carrierScore;
                            }
                        }
                    }
                }
            }
            return pawn;
        }
    }
}
