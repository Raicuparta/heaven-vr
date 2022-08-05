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
		    public BoneMapping(string path, Transform boneTransform = null)
		    {
			    Path = path;
			    BoneTransform = boneTransform;
		    }
		    
		    public string Path;
		    public Transform BoneTransform;
	    }
	    
	    private const string localPathBase = "localAvatarTrackers";
	    private const string globalPathBase = "LIV.avatarTrackers";
	    private List<PathfinderRigidTransform> pathfinderRigidTransforms;
	    private float previousHeight;
	    private float previousArmSpan;

	    [SerializeField] private List<BoneMapping> boneMappings = new List<BoneMapping>()
	    {
		    new BoneMapping("bob.stage.avatar.trackers.head"),
		    new BoneMapping("bob.stage.avatar.trackers.chest"),
			new BoneMapping("bob.stage.avatar.trackers.waist"),
			new BoneMapping("bob.stage.avatar.trackers.leftHand"),
			new BoneMapping("bob.stage.avatar.trackers.leftElbowGoal"),
			new BoneMapping("bob.stage.avatar.trackers.rightHand"),
			new BoneMapping("bob.stage.avatar.trackers.rightElbowGoal"),
			new BoneMapping("bob.stage.avatar.trackers.leftFoot"),
			new BoneMapping("bob.stage.avatar.trackers.leftKneeGoal"),
			new BoneMapping("bob.stage.avatar.trackers.rightFoot"),
			new BoneMapping("bob.stage.avatar.trackers.rightKneeGoal"),
	    };

	    [SerializeField] private Animator animator;
	    [SerializeField] private float playerHeight = 1.73f;
	    [SerializeField] private float playerArmSpan = 1.60f;

		public void Start()
		{
			pathfinderRigidTransforms = new List<PathfinderRigidTransform>();
	        foreach (var boneMapping in boneMappings)
	        {
		        pathfinderRigidTransforms.Add(CreatePathfinderTransform(boneMapping.BoneTransform, boneMapping.Path));
	        }
        }

        private PathfinderRigidTransform CreatePathfinderTransform(Transform boneTransform, string path)
        {
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
	        
	        if (Mathf.Abs(playerHeight - previousHeight) > 0.1f)
	        {
		        previousHeight = playerHeight;
				SDKBridgePathfinder.SetValue($"{localPathBase}.bob.stage.avatar.height", ref playerHeight, (int) PathfinderType.Float);
	        }
	        if (Mathf.Abs(playerArmSpan - previousArmSpan) > 0.1f)
	        {
		        previousArmSpan = playerArmSpan;
				SDKBridgePathfinder.SetValue($"{localPathBase}.bob.stage.avatar.armspan", ref playerArmSpan, (int) PathfinderType.Float);
	        }
	        
	        SDKBridgePathfinder.CopyPath(globalPathBase, localPathBase);
        }

        public void SetSpeed(float speed)
        {
	        animator.speed = speed;
        }
    }
}