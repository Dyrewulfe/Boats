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
using UnityEngine;
using UnityEngine.AI;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimShips.UI
{
    public class ITab_Ship_Cargo : ITab
    {
        public ITab_Ship_Cargo()
        {
            this.size = new Vector2(460f, 450f);
            this.labelKey = "TabCargo";
        }

        public override bool IsVisible => !this.SelPawnForCargo.GetComp<CompShips>().beached;

        private bool CanControl => !this.SelPawnForCargo.Downed && !this.SelPawnForCargo.Dead && this.SelPawnForCargo.Faction == Faction.OfPlayer && !this.SelPawnForCargo.GetComp<CompShips>().beached;

        private Pawn SelPawnForCargo
        {
            get
            {
                if(!(base.SelPawn is null) && !(base.SelPawn.TryGetComp<CompShips>() is null) )
                {
                    return base.SelPawn;
                }
                throw new InvalidOperationException("Cargo tab on non-pawn ship " + base.SelThing);
            }
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, TopPadding, this.size.x, this.size.y - TopPadding);
            Rect rect2 = rect.ContractedBy(10f);
            Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, position.width, position.height);
            Rect viewRect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            float num = 0f;
            this.TryDrawMassInfo(ref num, viewRect.width);
            if(this.IsVisible)
            {
                Widgets.ListSeparator(ref num, viewRect.width, "Cargo".Translate());
                ITab_Ship_Cargo.workingInvList.Clear();
                ITab_Ship_Cargo.workingInvList.AddRange(this.SelPawnForCargo.inventory.innerContainer);
                foreach(Thing t in ITab_Ship_Cargo.workingInvList)
                {
                    this.DrawThingRow(ref num, viewRect.width, t, true);
                }
                ITab_Ship_Cargo.workingInvList.Clear();
            }
            if(Event.current.type is EventType.Layout)
            {
                this.scrollViewHeight = num + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
        {
            Rect rect = new Rect(0f, y, width, ThingIconSize);
            Widgets.InfoCardButton(rect.width - 24f, y, thing);
            rect.width -= 24f;

            Rect rect2 = rect;
            rect2.xMin = rect2.xMax - 60f;
            CaravanThingsTabUtility.DrawMass(thing, rect2);
            rect.width -= 60f;

            if(Mouse.IsOver(rect))
            {
                GUI.color = ITab_Ship_Cargo.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if(!(thing.def.DrawMatSingle is null) && !(thing.def.DrawMatSingle.mainTexture is null))
            {
                Widgets.ThingIcon(new Rect(4f, y, ThingIconSize, ThingRowheight), thing, 1f);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Ship_Cargo.ThingLabelColor;
            Rect rect3 = new Rect(ThingLeftX, y, rect.width - ThingLeftX, rect.height);
            string text = thing.LabelCap;
            Text.WordWrap = false;
            Widgets.Label(rect3, text.Truncate(rect3.width, null));
            Text.WordWrap = true;
            string text2 = thing.DescriptionDetailed;
            if(thing.def.useHitPoints)
            {
                string text3 = text2;
                text2 = string.Concat(new object[]
                {
                    text3, "\n", thing.HitPoints, " / ", thing.MaxHitPoints
                });
            }
            TooltipHandler.TipRegion(rect, text2);
            y += ThingRowheight;
        }
        
        private void TryDrawMassInfo(ref float curY, float width)
        {
            if (this.SelPawnForCargo.GetComp<CompShips>().beached)
                return;
            Rect rect = new Rect(0f, curY, width, 22f);
            float num = MassUtility.GearAndInventoryMass(this.SelPawnForCargo);
            float num2 = MassUtility.Capacity(this.SelPawnForCargo, null);
            Widgets.Label(rect, "MassCarried".Translate(num.ToString("0.##"), num2.ToString("0.##")));
            curY += 22f;
        }

        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        private const float TopPadding = 20f;

        public static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        public static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private const float ThingIconSize = 28f;

        private const float ThingRowheight = 28f;

        private const float ThingLeftX = 36f;

        private const float StandardLineHeight = 22f;

        private static List<Thing> workingInvList = new List<Thing>();
    }
}
