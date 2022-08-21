using HarmonyLib;
using UnityEngine.EventSystems;

namespace HeavenVr.Laser.Patches;

[HarmonyPatch]
public static class LaserPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EventSystem), "OnEnable")]
    private static void AddLaserInputModule(EventSystem __instance)
    {
        if (__instance.name.Contains("UniverseLib")) return;

        LaserInputModule.Create(__instance);
        DefaultInputModuleDisabler.Create(__instance);
    }    
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TargetAssist), nameof(TargetAssist.LoadPrefs))]
    private static bool ForceDisableTargetAssist(TargetAssist __instance)
    {
        // TODO try to actually fix the target assist, or hide the option.
        // TODO also check if this one is actually doing anything when the lock-on option is off.
        __instance.MasterAssistIntensity = 0;
        return false;
    }
}