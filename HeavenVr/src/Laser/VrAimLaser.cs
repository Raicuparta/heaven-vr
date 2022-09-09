using System;
using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using HeavenVr.Stage;
using UnityEngine;

namespace HeavenVr.Laser;

public class VrAimLaser: MonoBehaviour
{
    private Transform _laserScaler;
    private Transform _crosshair;
    private Vector3 _initialLocalEuler;
    
    public static VrAimLaser Create(Transform hand)
    {
        var instance = hand.Find("Wrapper/Laser").GetOrAddComponent<VrAimLaser>();
        LayerHelper.SetLayerRecursive(instance.gameObject, GameLayer.VrUi);
        
        return instance;
    }

    private void Start()
    {
        _laserScaler = transform.Find("LaserScaler");
        _initialLocalEuler = transform.parent.localEulerAngles;
        SetUpCrosshair();
        SetUpMuzzleFlash();
    }

    private void OnEnable()
    {
        VrSettings.AimingAngleOffset.SettingChanged += OnAimingAngleSettingChanged;
    }

    private void OnDisable()
    {
        VrSettings.AimingAngleOffset.SettingChanged -= OnAimingAngleSettingChanged;
    }

    // TODO this should probably be in a VrHand component instead of here.
    private void OnAimingAngleSettingChanged(object sender, EventArgs e)
    {
        transform.parent.localEulerAngles = new Vector3(_initialLocalEuler.x + VrSettings.AimingAngleOffset.Value,
            _initialLocalEuler.y,
            _initialLocalEuler.z);
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