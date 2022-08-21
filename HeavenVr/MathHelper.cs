using UnityEngine;

namespace HeavenVr;

public static class MathHelper
{
    public static Vector3 GetProjectedForward(Transform transform)
    {
        var forward = transform.forward;
        forward.y = 0;
        return forward;
    }

    public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
    {
        // account for double-cover
        var dot = Quaternion.Dot(rot, target);
        var multi = dot > 0f ? 1f : -1f;
        target.x *= multi;
        target.y *= multi;
        target.z *= multi;
        target.w *= multi;
        // smooth damp (nlerp approx)
        var result = new Vector4(
            SmoothDamp(rot.x, target.x, ref deriv.x, time),
            SmoothDamp(rot.y, target.y, ref deriv.y, time),
            SmoothDamp(rot.z, target.z, ref deriv.z, time),
            SmoothDamp(rot.w, target.w, ref deriv.w, time)
        ).normalized;
        // compute deriv
        var dtInv = 1f / Time.unscaledDeltaTime;
        deriv.x = (result.x - rot.x) * dtInv;
        deriv.y = (result.y - rot.y) * dtInv;
        deriv.z = (result.z - rot.z) * dtInv;
        deriv.w = (result.w - rot.w) * dtInv;
        return new Quaternion(result.x, result.y, result.z, result.w);
    }

    private static float SmoothDamp(
        float current,
        float target,
        ref float currentVelocity,
        float smoothTime)
    {
        return Mathf.SmoothDamp(
            current,
            target,
            ref currentVelocity,
            smoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime);
    }
}