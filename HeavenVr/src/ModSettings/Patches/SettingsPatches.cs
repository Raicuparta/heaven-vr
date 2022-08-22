using HarmonyLib;

namespace HeavenVr.ModSettings.Patches;

[HarmonyPatch]
public static class SettingsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntroCard), nameof(IntroCard.IsDone))]
    private static bool SkipIntro(ref bool __result)
    {
        // TODO read this from a setting.
        __result = true;
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuScreenDialogue), nameof(MenuScreenDialogue.OnFastForwardButtonToggled))]
    private static void SpeedDialogueSkip(MenuScreenDialogue __instance)
    {
        __instance.ffTimescale = 100f;
    }
}