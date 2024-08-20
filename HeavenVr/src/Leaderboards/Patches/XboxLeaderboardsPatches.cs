using HarmonyLib;

namespace HeavenVr.Leaderboards.Patches;

// We don't use [HarmonyPatch] because this is patched conditionally from HeavenVrPlugin.cs
public static class XboxLeaderboardsPatches
{
    [HarmonyPrefix]
    // We find by string since these dependencies aren't in the Steam version used for dev.
    [HarmonyPatch("LeaderboardIntegrationBitcode, Assembly-CSharp", "LoadLeaderboardAndConditionallySubmitScore")]
    private static void SkipLeaderboards(ref bool uploadScore)
    {
        // Skip the leaderboards on Xbox, since I don't know if it's possible to create new ones on the fly
        // like I do on the Steam version. I don't want players to mix their VR scores with flat scores.
        uploadScore = false;
    }
}