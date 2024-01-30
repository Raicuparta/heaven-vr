using System.Linq;
using HarmonyLib;
using HeavenVr.Stage;
using UnityEngine;

namespace HeavenVr.Weapons.Patches;

[HarmonyPatch]
public static class WeaponPatches
{
    private static readonly string[] ProjectileIds =
    {
        "Projectiles/ProjectileBomb",
        "Projectiles/ProjectileMine",
        "Projectiles/ProjectileRocketFast"
    };

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.CreateProjectile), typeof(string), typeof(Vector3),
        typeof(Vector3), typeof(ProjectileWeapon))]
    private static void AimProjectilesWithVrLaser(string path, ref Vector3 origin, ref Vector3 forward)
    {
        if (!VrStage.Instance || !VrStage.Instance.aimLaser || !ProjectileIds.Contains(path)) return;

        origin = VrStage.Instance.aimLaser.transform.position;
        forward = VrStage.Instance.aimLaser.transform.forward;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Camera), nameof(Camera.ViewportPointToRay), typeof(Vector3))]
    private static void UseVrAimingDirection(Vector3 pos, ref Ray __result, Camera __instance)
    {
        if (!__instance.CompareTag("MainCamera") || !VrStage.Instance || !VrStage.Instance.aimLaser) return;

        var laserTransform = VrStage.Instance.aimLaser.transform;

        var centerDirection = __instance.ViewportPointToRay(Vector3.one * 0.5f, Camera.MonoOrStereoscopicEye.Mono)
            .direction;
        var originalDirection = __result.direction;
        var spreadOffset = originalDirection - centerDirection;
        var newDirection = laserTransform.forward + spreadOffset;

        __result = new Ray(laserTransform.position, newDirection);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.SetVisualOffsetAmount))]
    private static bool AdjustProjectileVisualOffset(ProjectileBase __instance)
    {
        if (__instance._damageTarget != ProjectileBase.DamageTarget.Damageable) return true;

        __instance._projectileModelHolder.localPosition = Vector3.forward * 1.5f;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.OnSpawn))]
    private static void AdjustProjectileCameraOffset(ProjectileBase __instance)
    {
        if (__instance._damageTarget != ProjectileBase.DamageTarget.Damageable) return;

        __instance._spawnCameraOffset = Vector3.zero;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MechController), nameof(MechController.GetTelefragTarget))]
    private static void PreventRotatingAfterTelefrag(MechController.TelefragTarget __result)
    {
        if (__result == null) return;

        __result.wasPreviousTarget = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Zipline), nameof(Zipline.UpdateRope))]
    private static void AttachZiplineToHand(Zipline __instance)
    {
        __instance.ropeCameraOffset = VrStage.Instance.aimLaser.transform.position -
                                      RM.mechController.playerCamera.transform.position;
    }
}