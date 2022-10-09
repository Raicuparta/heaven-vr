﻿using HarmonyLib;
using HeavenVr.ModSettings;
using Steamworks;

namespace HeavenVr.Leaderboards.Patches;

[HarmonyPatch]
public static class LeaderboardsPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataManager), nameof(GameDataManager.OnReadPowerPrefsComplete))]
    private static void PreventSubmittingScores(GameDataManager __instance)
    {
        GameDataManager.powerPrefs.dontUploadToLeaderboard = !VrSettings.EnableLeaderboards.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamUserStats), nameof(SteamUserStats.FindOrCreateLeaderboard))]
    [HarmonyPatch(typeof(SteamUserStats), nameof(SteamUserStats.FindLeaderboard))]
    private static void UseVrLeaderboards(ref string pchLeaderboardName)
    {
        // Adding this suffix to the leaderboard names will make it so there's a VR variant of every leaderboard  the
        // game tries to create. So VR players compete only with other VR players.
        // These dynamically-created leaderboards won't show up in the Steam community pages, but they will show up
        // in-game, replacing the original leaderboards.
        pchLeaderboardName = $"{pchLeaderboardName}__VR";
    }
}