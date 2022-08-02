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
	    private static readonly Dictionary<string, string> boneMap = new Dictionary<string, string>()
		{
			{ "B-forearm.R", "stage.avatar.trackers.rightElbowGoal" },
			{ "B-toe.L", "stage.avatar.trackers.leftFoot" },
			{ "B-shin.L", "stage.avatar.trackers.leftKneeGoal" },
			{ "B-toe.R", "stage.avatar.trackers.rightFoot" },
			{ "B-shin.R", "stage.avatar.trackers.rightKneeGoal" },
		};

	    [SerializeField] private Animator animator;

		public void Start()
		{
			pathfinderRigidTransforms = new List<PathfinderRigidTransform>();
	        var children = gameObject.GetComponentsInChildren<Transform>();
			foreach (var child in children)
			{
				if (boneMap.ContainsKey(child.name))
				{
					pathfinderRigidTransforms.Add(CreatePathfinderTransform(child, boneMap[child.name]));
				}
			}
        }

        private PathfinderRigidTransform CreatePathfinderTransform(Transform child, string path)
        {
	        var pathfinderTransform = new GameObject($"Pathfinder-{child.name}").AddComponent<PathfinderRigidTransform>();
			pathfinderTransform.transform.SetParent(child, false);
	        pathfinderTransform.Root = transform;
			pathfinderTransform.Key = path;
			pathfinderTransform.PathBase = localPathBase;

			if (child.name.StartsWith("B-toe"))
			{
				pathfinderTransform.transform.localEulerAngles = new Vector3(0f, 0, 90f);
			}
			else if (child.name.EndsWith(".L"))
			{
				pathfinderTransform.transform.localEulerAngles = new Vector3(0f, 0, -90f);
			}
			else if (child.name == "ROOTSHJnt")
			{
				pathfinderTransform.transform.localEulerAngles = new Vector3(90f, -90f, 0);
			}

			return pathfinderTransform;
        }
        
        private void Update()
        {
	        if (pathfinderRigidTransforms == null) return;

	        foreach (var pathfinderRigidTransform in pathfinderRigidTransforms)
	        {
		        pathfinderRigidTransform.SetPathfinderValuesLocally();
	        }

	        SDKBridgePathfinder.CopyPath(globalPathBase, localPathBase);
        }

        public void SetSpeed(float speed)
        {
	        animator.speed = speed;
        }
    }
}