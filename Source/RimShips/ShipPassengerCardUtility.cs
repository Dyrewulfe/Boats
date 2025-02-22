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

namespace RimShips.UI
{
    [StaticConstructorOnStartup]
    public static class ShipPassengerCardUtility
    {
        public static void DrawShipPassengersCard(Rect outRect, Pawn pawn, List<Pawn> passengers)
        {
            if(pawn.GetComp<CompShips>() is null)
            {
                Log.Error("Called DrawShipPassengersCard with a non ship pawn. Please stop messing with my files... thanks.");
            }
            outRect = outRect.Rounded();
            Rect rect = new Rect(outRect.x, outRect.y, outRect.width * 0.375f, outRect.height).Rounded();
            Rect rect2 = new Rect(rect.xMax, outRect.y, outRect.width - rect.width, outRect.height);
            rect.yMin += 11f;
            DrawPassengerList(rect2, pawn, passengers);
        }

        public static void DrawPassengerList(Rect rect, Pawn pawn, List<Pawn> passengers)
        {
            GUI.color = Color.white;

            GUI.BeginGroup(rect);
            float lineHeight = Text.LineHeight;
            Rect outRect = new Rect(0f, 0f, rect.width, rect.height - lineHeight);
            //Rect viewRect = new Rect(0f, 0f, rect.width - 16f, HealthCardUtility.scrollViewHeight);
        }
    }
}
