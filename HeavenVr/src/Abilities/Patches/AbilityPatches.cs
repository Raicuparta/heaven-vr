using HarmonyLib;
using HeavenVr.Stage;
using UnityEngine;

namespace HeavenVr.Abilities.Patches;

[HarmonyPatch]
public abstract class AbilityPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirstPersonDrifter), nameof(FirstPersonDrifter.ForceDash))]
    private static void FixAbilityDirection(ref Vector3 newDashDirection, ref Vector3 newDashEndVelocity)
    {
        if (!VrStage.Instance || !VrStage.Instance.aimLaser || !RM.mechController) return;

        float endVelocity;
        var direction = VrStage.Instance.aimLaser.transform.forward;
        
        switch (RM.mechController.m_lastDiscardAbility)
        {
            case PlayerCardData.DiscardAbility.Dash:
            {
                endVelocity = RM.mechController.abilityDashEndVelocity;
                direction.y = 0;
                break;
            }
            case PlayerCardData.DiscardAbility.ShieldBash:
            {
                endVelocity = RM.mechController.abilityShieldBashEndVelocity;
                direction.y = 0;
                break;
            }
            case PlayerCardData.DiscardAbility.Fireball:
            {
                endVelocity = RM.mechController.abilityFireballEndVelocity;
                break;
            }
            default:
            {
                return;
            }
        }
        
        newDashDirection = direction;
        newDashEndVelocity = direction * endVelocity;
    }
}