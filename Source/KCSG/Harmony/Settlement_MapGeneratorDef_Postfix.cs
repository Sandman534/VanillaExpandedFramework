﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    public class Settlement_MapGeneratorDef_Postfix
    {
        [HarmonyPatch(typeof(Settlement), nameof(Settlement.MapGeneratorDef), MethodType.Getter)]
        public static class Settlement_MapGeneratorDef_Patch
        {
            public static void Postfix(Settlement __instance, ref MapGeneratorDef __result)
            {
                if (__instance != null && __instance.Faction != null && __instance.Faction != Faction.OfPlayer)
                {
                    if (__instance.Faction.def.HasModExtension<CustomGenOption>())
                    {
                        Debug.Message($"Generating base for faction: {__instance.Faction.NameColored}");
                        var ext = __instance.Faction.def.GetModExtension<CustomGenOption>();
                        __result = ext.preventBridgeable ? DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction_NoBridge") : DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction");
                    }
                    else if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == __instance.Tile && o.def.HasModExtension<CustomGenOption>()) is WorldObject wo)
                    {
                        Debug.Message($"Generating world object map");
                        var ext = wo.def.GetModExtension<CustomGenOption>();
                        __result = ext.preventBridgeable ? DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen_NoBridge") : DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject_Gen");
                    }
                }
            }
        }
    }
}