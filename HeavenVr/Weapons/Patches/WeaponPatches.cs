using HarmonyLib;
using HeavenVr.Stage;
using UnityEngine;
using System.Linq;

namespace HeavenVr.Weapons.Patches;

[HarmonyPatch]
public static class WeaponPatches
{
    private static readonly string[] ProjectileIds = {
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
        
        var centerDirection = __instance.ViewportPointToRay(Vector3.one * 0.5f, Camera.MonoOrStereoscopicEye.Mono).direction;
        var originalDirection = __result.direction;
        var spreadOffset = originalDirection - centerDirection;
        var newDirection = laserTransform.forward + spreadOffset;
        
        __result = new Ray(laserTransform.position, newDirection);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.SetVisualOffsetAmount))]
    private static bool AdjustProjectileVisualOffset(ProjectileBase __instance)
    {
        __instance._projectileModelHolder.localPosition = Vector3.forward * 1.5f;
        return false;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileBase), nameof(ProjectileBase.OnSpawn))]
    private static void AdjustProjectileCameraOffset(ProjectileBase __instance)
    {
        __instance._spawnCameraOffset = Vector3.zero;
    }
}