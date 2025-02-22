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

namespace RimShips
{
    [StaticConstructorOnStartup]
    public static class TexCommandShips
    {
        public static readonly Texture2D UnloadAll = ContentFinder<Texture2D>.Get("UI/UnloadAll", true);

        public static readonly Texture2D UnloadPassenger = ContentFinder<Texture2D>.Get("UI/UnloadPassenger", true);

        public static readonly Texture2D UnloadCaptain = ContentFinder<Texture2D>.Get("UI/UnloadCaptain", true);

        public static readonly Texture2D Anchor = ContentFinder<Texture2D>.Get("UI/Anchor", true);

        public static readonly Texture2D OnboardIcon = ContentFinder<Texture2D>.Get("UI/OnboardIcon", true);
    }
}