using System.Collections.Generic;
using BoneworksLIV.AvatarTrackers;
using UnityEngine;

namespace LIV.AvatarTrackers
{
    public class PathfinderAvatarTrackers: MonoBehaviour
    {
	    private const string localPathBase = "localAvatarTrackers";
	    private const string globalPathBase = "LIV.avatarTrackers";
	    private List<PathfinderRigidTransform> pathfinderRigidTransforms;
	    private static readonly Dictionary<HumanBodyBones, string> boneMap = new Dictionary<HumanBodyBones, string>()
		{
			{ HumanBodyBones.Head, "bob.stage.avatar.trackers.head" }, // other options: Neck_01SHJnt / Neck_02SHJnt / Neck_TopSHJnt / Head_JawSHJnt / Head_TopSHJnt
			{ HumanBodyBones.UpperChest, "bob.stage.avatar.trackers.chest" }, // TODO: chest looks hella broken, better off not tracking it at all.
			{ HumanBodyBones.Hips, "bob.stage.avatar.trackers.waist" },
			{ HumanBodyBones.LeftHand, "bob.stage.avatar.trackers.leftHand" }, // other options:  l_Hand_1SHJnt / l_Hand_2SHJnt / l_GripPoint_AuxSHJnt
			{ HumanBodyBones.LeftLowerArm, "bob.stage.avatar.trackers.leftElbowGoal" },
			{ HumanBodyBones.RightHand, "bob.stage.avatar.trackers.rightHand" }, // other options: r_Hand_1SHJnt / r_Hand_2SHJnt / r_GripPoint_AuxSHJnt
			{ HumanBodyBones.RightLowerArm, "bob.stage.avatar.trackers.rightElbowGoal" },
			{ HumanBodyBones.LeftToes, "bob.stage.avatar.trackers.leftFoot" }, // other options:  l_Leg_BallSHJnt
			{ HumanBodyBones.LeftLowerLeg, "bob.stage.avatar.trackers.leftKneeGoal" },
			{ HumanBodyBones.RightToes, "bob.stage.avatar.trackers.rightFoot" }, // other options: r_Leg_BallSHJnt
			{ HumanBodyBones.RightLowerLeg, "bob.stage.avatar.trackers.rightKneeGoal" },
		};

	    [SerializeField] private Animator animator;

		public void Start()
		{
			pathfinderRigidTransforms = new List<PathfinderRigidTransform>();
	        foreach (var boneMapping in boneMap)
	        {
		        pathfinderRigidTransforms.Add(CreatePathfinderTransform(boneMapping.Key, boneMapping.Value));
	        }
        }

        private PathfinderRigidTransform CreatePathfinderTransform(HumanBodyBones bone, string path)
        {
	        var boneTransform = animator.GetBoneTransform(bone);
	        var pathfinderTransform = new GameObject($"Pathfinder-{boneTransform.name}").AddComponent<PathfinderRigidTransform>();
			pathfinderTransform.transform.SetParent(boneTransform, false);
	        pathfinderTransform.Root = transform;
			pathfinderTransform.Key = path;
			pathfinderTransform.PathBase = localPathBase;

			pathfinderTransform.transform.localEulerAngles = new Vector3(0f, 0, 90f);

			return pathfinderTransform;
        }
        
        private void Update()
        {
	        if (pathfinderRigidTransforms == null) return;

	        foreach (var pathfinderRigidTransform in pathfinderRigidTransforms)
	        {
		        pathfinderRigidTransform.SetPathfinderValuesLocally();
	        }

	        Debug.Log("copy");
	        
	        SDKBridgePathfinder.CopyPath(globalPathBase, localPathBase);
        }

        public void SetSpeed(float speed)
        {
	        animator.speed = speed;
        }
    }
}