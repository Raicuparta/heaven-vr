using System;
using System.Collections.Generic;
using BoneworksLIV.AvatarTrackers;
using JetBrains.Annotations;
using UnityEngine;

namespace LIV.AvatarTrackers
{
    public class PathfinderAvatarTrackers: MonoBehaviour
    {
	    [Serializable]
	    public struct BoneMapping
	    {
		    public BoneMapping(string path, Transform boneTransform = null, Vector3 eulerOffset = default)
		    {
			    Path = path;
			    BoneTransform = boneTransform;
			    EulerOffset = eulerOffset;
		    }
		    
		    public string Path;
		    public Transform BoneTransform;
		    public Vector3 EulerOffset;
	    }
	    
	    private const string localPathBase = "localAvatarTrackers";
	    private const string globalPathBase = "LIV.avatarTrackers";
	    private List<PathfinderRigidTransform> pathfinderRigidTransforms;
	    private float previousHeight;
	    private float previousArmSpan;

	    [SerializeField] private List<BoneMapping> boneMappings = new List<BoneMapping>()
	    {
		    new BoneMapping("stage.avatar.trackers.head"),
		    new BoneMapping("stage.avatar.trackers.chest"),
			new BoneMapping("stage.avatar.trackers.waist"),
			new BoneMapping("stage.avatar.trackers.leftHand"),
			new BoneMapping("stage.avatar.trackers.leftElbowGoal"),
			new BoneMapping("stage.avatar.trackers.rightHand"),
			new BoneMapping("stage.avatar.trackers.rightElbowGoal"),
			new BoneMapping("stage.avatar.trackers.leftFoot"),
			new BoneMapping("stage.avatar.trackers.leftKneeGoal"),
			new BoneMapping("stage.avatar.trackers.rightFoot"),
			new BoneMapping("stage.avatar.trackers.rightKneeGoal"),
	    };

	    [SerializeField] private Animator animator;
	    [SerializeField] private float playerHeight = 1.73f;
	    [SerializeField] private float playerArmSpan = 1.60f;

		public void Start()
		{
			pathfinderRigidTransforms = new List<PathfinderRigidTransform>();
	        foreach (var boneMapping in boneMappings)
	        {
		        pathfinderRigidTransforms.Add(CreatePathfinderTransform(boneMapping));
	        }
        }

        private PathfinderRigidTransform CreatePathfinderTransform(BoneMapping boneMapping)
        {
	        var pathfinderTransform = new GameObject($"Pathfinder-{boneMapping.BoneTransform.name}").AddComponent<PathfinderRigidTransform>();
			pathfinderTransform.transform.SetParent(boneMapping.BoneTransform, false);
	        pathfinderTransform.Root = transform;
			pathfinderTransform.Key = boneMapping.Path;
			pathfinderTransform.PathBase = localPathBase;

			pathfinderTransform.transform.localEulerAngles = boneMapping.EulerOffset;
			

			return pathfinderTransform;
        }
        
        private void Update()
        {
	        if (pathfinderRigidTransforms == null) return;

	        foreach (var pathfinderRigidTransform in pathfinderRigidTransforms)
	        {
		        pathfinderRigidTransform.SetPathfinderValuesLocally();
	        }
	        
	        if (Mathf.Abs(playerHeight - previousHeight) > 0.1f)
	        {
		        previousHeight = playerHeight;
				SDKBridgePathfinder.SetValue($"{localPathBase}.stage.avatar.height", ref playerHeight, (int) PathfinderType.Float);
	        }
	        if (Mathf.Abs(playerArmSpan - previousArmSpan) > 0.1f)
	        {
		        previousArmSpan = playerArmSpan;
				SDKBridgePathfinder.SetValue($"{localPathBase}.stage.avatar.armspan", ref playerArmSpan, (int) PathfinderType.Float);
	        }
	        
	        SDKBridgePathfinder.CopyPath(globalPathBase, localPathBase);
        }

        public void SetSpeed(float speed)
        {
	        animator.speed = speed;
        }
    }
}