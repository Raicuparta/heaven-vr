using HarmonyLib;

namespace HeavenVr.ModSettings.Patches;

[HarmonyPatch]
public static class SettingsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntroCard), nameof(IntroCard.IsDone))]
    private static bool SkipIntro(ref bool __result)
    {
        if (!VrSettings.SkipIntro.Value) return true;

        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuScreenDialogue), nameof(MenuScreenDialogue.OnFastForwardButtonToggled))]
    private static void SpeedDialogueSkip(MenuScreenDialogue __instance)
    {
        __instance.ffTimescale = 20f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataManager), nameof(GameDataManager.OnReadPowerPrefsComplete))]
    private static void PreventSubmittingScores(GameDataManager __instance)
    {
        // Since the VR mod changes a lot of how the game works,
        // it wouldn't make sense to submit scores to the same leaderboard.
        // For now I'm just disabling score submission altogether.
        // TODO: submit vr scores to a separate leaderboard.
        GameDataManager.powerPrefs.dontUploadToLeaderboard = true;
    }
}