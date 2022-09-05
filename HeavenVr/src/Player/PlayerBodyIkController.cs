using System;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine;

namespace HeavenVr.Player;

public class PlayerBodyIkController: MonoBehaviour
{
    private VRIK _vrIk;
    
    public static void Create(Transform camera, Transform leftHand, Transform rightHand)
    {
        var instance = Instantiate(VrAssetLoader.PlayerBodyIk, camera.parent, false).AddComponent<PlayerBodyIkController>();

        var headTarget = new GameObject("VrIkHeadTarget").transform;
        headTarget.SetParent(camera, false);
        headTarget.localPosition = new Vector3(0f, -0.08f, -0.15f);
        headTarget.localEulerAngles = new Vector3(0f, 90f, -90f);
        
        var leftHandTarget = new GameObject("VrIkLeftHandTarget").transform;
        leftHandTarget.SetParent(leftHand, false);
        leftHandTarget.localPosition = new Vector3(-0.0364f, 0.1455f, -0.0327f);
        leftHandTarget.localEulerAngles = new Vector3(0, 0f, 90f);
        
        var rightHandTarget = new GameObject("VrIkRightHandTarget").transform;
        rightHandTarget.SetParent(rightHand, false);
        rightHandTarget.localPosition = new Vector3(0.0364f, 0.1455f, -0.0327f);
        rightHandTarget.localEulerAngles = new Vector3(0f, 180f, -90f);

        var root = instance.transform.Find("WhiteRig_SHJntGrp");

        var allBones = root.GetComponentsInChildren<Transform>();
        
        var vrIk = instance.GetComponent<VRIK>();
        instance._vrIk = vrIk;
        
        vrIk.references.root = root;
        vrIk.references.pelvis = allBones.First(bone => bone.name == "WhiteRig_ROOTSHJnt");
        vrIk.references.spine = allBones.First(bone => bone.name == "WhiteRig_Spine_01SHJnt");
        vrIk.references.chest = allBones.First(bone => bone.name == "WhiteRig_Spine_02SHJnt");
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
        vrIk.solver.leftArm.target = leftHandTarget;
        vrIk.solver.rightArm.target = rightHandTarget;
        vrIk.solver.plantFeet = false;
        vrIk.solver.locomotion.stepThreshold = 0.1f;
        vrIk.solver.locomotion.stepSpeed = 10f;
        vrIk.solver.locomotion.maxVelocity = 3f;
        vrIk.solver.locomotion.stepSpeed = 6f;

        vrIk.transform.localScale = Vector3.one * 2f; // TODO calculate scale.

        if (RM.drifter)
        {
            instance.transform.SetParent(RM.drifter.transform, false);
            instance.transform.localPosition = Vector3.up * -2f;
        }

    }

    private void Update()
    {
        if (!RM.drifter) return;

        if (RM.drifter.grounded)
        {
            transform.localPosition = Vector3.up * -2f;
        }
        else
        {
            transform.localPosition = Vector3.up * -6f;
        }
    }
}