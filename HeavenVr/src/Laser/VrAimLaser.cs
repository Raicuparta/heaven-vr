using System;
using HeavenVr.Helpers;
using HeavenVr.Stage;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace HeavenVr.Laser;

public class VrAimLaser: MonoBehaviour
{
    private Transform _laserScaler;
    private Transform _crosshair;
    
    public static VrAimLaser Create(Transform hand)
    {
        var instance = hand.Find("Laser").GetOrAddComponent<VrAimLaser>();
        LayerHelper.SetLayerRecursive(instance.gameObject, GameLayer.VrUi);
        
        return instance;
    }

    private void Start()
    {
        _laserScaler = transform.Find("LaserScaler");
        SetUpCrosshair();
        SetUpMuzzleFlash();
    }

    public void SetDistance(float distance)
    {
        var scale = _laserScaler.localScale;
        scale.z = distance / transform.lossyScale.z;
        _laserScaler.localScale = scale;
    }

    private void SetUpCrosshair()
    {
        if (!RM.ui || !RM.ui.crosshair) return;

        _crosshair = RM.ui.crosshair.parent;
        
        _crosshair.SetParent(transform, false);
        _crosshair.localScale = Vector3.one * 0.1f;
        _crosshair.localPosition = Vector3.forward;
        LayerHelper.SetLayerRecursive(RM.ui.crosshair.parent.gameObject, GameLayer.Default);
    }

    private void SetUpMuzzleFlash()
    {
        if (!RM.mechController || !RM.mechController.muzzleFlashController) return;

        var muzzleFlash = RM.mechController.muzzleFlashController.transform;
        
        muzzleFlash.SetParent(transform, false);
        muzzleFlash.localScale = Vector3.one * 0.1f;
        muzzleFlash.localPosition = Vector3.forward;
    }

    private void Update()
    {
        if (_crosshair == null || VrStage.Instance == null || VrStage.Instance.VrCamera == null) return;
        
        _crosshair.rotation = Quaternion.LookRotation(_crosshair.position - transform.position, VrStage.Instance.VrCamera.transform.up);
    }
}