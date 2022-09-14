using HarmonyLib;
using HeavenVr.Helpers;
using UnityEngine;

namespace HeavenVr.Player.Patches;

/*
* There are a bunch of triggers in the game that don't check if the collider that entered the trigger
* belongs to the player object. This mod may add some extra colliders for VR, so we need to make sure
* no other collider is detected by these triggers.
*/
[HarmonyPatch]
public static class ColliderPatches
{
    private static bool IsPlayer(Component component)
    {
        return component.CompareTag(GameTag.Player);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardPickup), nameof(CardPickup.OnTriggerStay))]
    private static bool PreventCradPickupWithNonPlayerColliders(Collider c)
    {
        return IsPlayer(c);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LevelGate), nameof(LevelGate.OnTriggerStay))]
    [HarmonyPatch(typeof(LevelGateBookOfLife), nameof(LevelGateBookOfLife.OnTriggerStay))]
    private static bool PreventLevelCompletionWithNonPlayerColliders(Collider other)
    {
        return IsPlayer(other);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AirstrikeSubzone), nameof(AirstrikeSubzone.OnTriggerEnter))]
    [HarmonyPatch(typeof(AirstrikeSubzone), nameof(AirstrikeSubzone.OnTriggerExit))]
    [HarmonyPatch(typeof(AirstrikeZone), nameof(AirstrikeZone.OnTriggerEnter))]
    [HarmonyPatch(typeof(AirstrikeZone), nameof(AirstrikeZone.OnTriggerExit))]
    [HarmonyPatch(typeof(EnvironmentPortalTrigger), nameof(EnvironmentPortalTrigger.OnTriggerEnter))]
    [HarmonyPatch(typeof(FoliageTrigger), nameof(FoliageTrigger.OnTriggerEnter))]
    [HarmonyPatch(typeof(GhostHintOriginVFX), nameof(GhostHintOriginVFX.OnTriggerEnter))]
    [HarmonyPatch(typeof(IdiotIslandAchievement), nameof(IdiotIslandAchievement.OnTriggerEnter))]
    [HarmonyPatch(typeof(PlayerWinTrigger), nameof(PlayerWinTrigger.OnTriggerEnter))]
    private static bool PreventTriggerWithNonPlayerColliders(Collider other)
    {
        return IsPlayer(other);
    }
}