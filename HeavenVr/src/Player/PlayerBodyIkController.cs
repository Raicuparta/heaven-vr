﻿using System;
using System.Linq;
using HeavenVr.Helpers;
using HeavenVr.ModSettings;
using RootMotion.FinalIK;
using UnityEngine;

namespace HeavenVr.Player;

public class PlayerBodyIkController : MonoBehaviour
{
    private SkinnedMeshRenderer _renderer;
    private VRIK _vrIk;
    private Transform _dominantHand;
    private Transform _nonDominantHand;
    private Transform _leftHandTarget;
    private Transform _rightHandTarget;

    public static void Create(Transform camera, Transform dominantHand, Transform nonDominantHand)
    {
        var instance = Instantiate(VrAssetLoader.PlayerBodyIk, camera.parent, false)
            .AddComponent<PlayerBodyIkController>();

        instance._dominantHand = dominantHand;
        instance._nonDominantHand = nonDominantHand;

        var headTarget = new GameObject("VrIkHeadTarget").transform;
        headTarget.SetParent(camera, false);
        headTarget.localPosition = new Vector3(0f, -0.08f, -0.15f);
        headTarget.localEulerAngles = new Vector3(0f, 90f, -90f);

        var root = instance.transform.Find("WhiteRig_SHJntGrp");

        var allBones = root.GetComponentsInChildren<Transform>();

        var vrIk = instance.GetComponent<VRIK>();
        instance._vrIk = vrIk;

        vrIk.references.root = root;
        vrIk.references.pelvis = allBones.First(bone => bone.name == "WhiteRig_ROOTSHJnt");
        vrIk.references.spine = allBones.First(bone => bone.name == "WhiteRig_Spine_02SHJnt");
        vrIk.references.chest = allBones.First(bone => bone.name == "WhiteRig_Spine_TopSHJnt");
        vrIk.references.neck = allBones.First(bone => bone.name == "WhiteRig_Neck_AuxSHJnt");
        vrIk.references.head = allBones.First(bone => bone.name == "WhiteRig_Head_TopSHJnt");
        vrIk.references.leftShoulder = allBones.First(bone => bone.name == "WhiteRig_l_Arm_ClavicleSHJnt");
        vrIk.references.leftUpperArm = allBones.First(bone => bone.name == "WhiteRig_l_Arm_ShoulderSHJnt");
        vrIk.references.leftForearm = allBones.First(bone => bone.name == "WhiteRig_l_Arm_ElbowSHJnt");
        vrIk.references.leftHand = allBones.First(bone => bone.name == "WhiteRig_l_Arm_WristSHJnt");
        vrIk.references.rightShoulder = allBones.First(bone => bone.name == "WhiteRig_r_Arm_ClavicleSHJnt");
        vrIk.references.rightUpperArm = allBones.First(bone => bone.name == "WhiteRig_r_Arm_ShoulderSHJnt");
        vrIk.references.rightForearm = allBones.First(bone => bone.name == "WhiteRig_r_Arm_ElbowSHJnt");
        vrIk.references.rightHand = allBones.First(bone => bone.name == "WhiteRig_r_Arm_WristSHJnt");
        vrIk.references.leftThigh = allBones.First(bone => bone.name == "WhiteRig_l_Leg_HipSHJnt");
        vrIk.references.leftCalf = allBones.First(bone => bone.name == "WhiteRig_l_Leg_KneeSHJnt");
        vrIk.references.leftFoot = allBones.First(bone => bone.name == "WhiteRig_l_Leg_AnkleSHJnt");
        vrIk.references.leftToes = allBones.First(bone => bone.name == "WhiteRig_l_Leg_BallSHJnt");
        vrIk.references.rightThigh = allBones.First(bone => bone.name == "WhiteRig_r_Leg_HipSHJnt");
        vrIk.references.rightCalf = allBones.First(bone => bone.name == "WhiteRig_r_Leg_KneeSHJnt");
        vrIk.references.rightFoot = allBones.First(bone => bone.name == "WhiteRig_r_Leg_AnkleSHJnt");
        vrIk.references.rightToes = allBones.First(bone => bone.name == "WhiteRig_r_Leg_BallSHJnt");

        vrIk.solver.spine.headTarget = headTarget;
        vrIk.solver.plantFeet = false;
        vrIk.solver.locomotion.stepThreshold = 0.1f;
        vrIk.solver.locomotion.stepSpeed = 10f;
        vrIk.solver.locomotion.maxVelocity = 3f;
        vrIk.solver.locomotion.stepSpeed = 6f;

        vrIk.transform.localScale = Vector3.one; // TODO calculate scale.

        if (RM.drifter)
        {
            instance.transform.SetParent(RM.drifter.transform, false);
            instance.transform.localPosition = Vector3.up * -2f;
        }

        instance._renderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        UpdateHandedness();
    }

    private void OnEnable()
    {
        VrSettings.LeftHandedMode.SettingChanged += OnHandednessChanged;
    }

    private void OnDisable()
    {
        VrSettings.LeftHandedMode.SettingChanged -= OnHandednessChanged;
    }

    private void Update()
    {
        UpdateRendererVisibility();
        UpdateLocomotion();
        UpdatePosition();
    }

    private void OnHandednessChanged(object sender, EventArgs e)
    {
        UpdateHandedness();
    }

    private void UpdateHandedness()
    {
        var isLeftHanded = VrSettings.LeftHandedMode.Value;
        
        if (_leftHandTarget == null)
        {
            _leftHandTarget = new GameObject("VrIkLeftHandTarget").transform;
            _vrIk.solver.leftArm.target = _leftHandTarget;
        }

        _leftHandTarget.SetParent(isLeftHanded ? _dominantHand : _nonDominantHand);
        _leftHandTarget.localPosition = new Vector3(-0.0364f, 0.1455f, -0.0327f);
        _leftHandTarget.localEulerAngles = new Vector3(0, 0f, 90f);

        if (_rightHandTarget == null)
        {
            _rightHandTarget = new GameObject("VrIkRightHandTarget").transform;
            _vrIk.solver.rightArm.target = _rightHandTarget;
        }

        _rightHandTarget.SetParent(isLeftHanded ? _nonDominantHand : _dominantHand);
        _rightHandTarget.localPosition = new Vector3(0.0364f, 0.1455f, -0.0327f);
        _rightHandTarget.localEulerAngles = new Vector3(0f, 180f, -90f);
    }

    private void UpdatePosition()
    {
        if (RM.drifter == null) return;

        transform.position = RM.drifter.GetFeetPosition();
    }

    private void UpdateRendererVisibility()
    {
        _renderer.enabled = !PauseHelper.IsPaused() && VrSettings.ShowPlayerBody.Value;
    }

    private void UpdateLocomotion()
    {
        if (!RM.drifter || PauseHelper.IsPaused()) return;

        if (RM.drifter.MovementVelocity.sqrMagnitude <= 0.1f)
        {
            _vrIk.solver.locomotion.maxVelocity = 0.1f;
            _vrIk.solver.locomotion.stepSpeed = 2f;
        }
        else if (RM.drifter.grounded)
        {
            _vrIk.solver.locomotion.maxVelocity = 4f;
            _vrIk.solver.locomotion.stepSpeed = 6f;
        }
        else
        {
            _vrIk.solver.locomotion.maxVelocity = 0f;
            _vrIk.solver.locomotion.stepSpeed = 4f;
        }
    }
}