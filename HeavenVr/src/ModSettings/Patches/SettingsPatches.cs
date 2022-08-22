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
    private static void SkipGodAwfulDialogue(MenuScreenDialogue __instance)
    {
        // TODO still need to actually force the skip option, this just speeds it up.
        __instance.ffTimescale = 100f;
    }
}