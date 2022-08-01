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

		public void SetUp(Transform root, IReadOnlyDictionary<string, string> boneMap)
		{
			pathfinderRigidTransforms = new List<PathfinderRigidTransform>();
	        var children = root.GetComponentsInChildren<Transform>();
			foreach (var child in children)
			{
				if (boneMap.ContainsKey(child.name))
				{
					pathfinderRigidTransforms.Add(CreatePathfinderTransform(child, boneMap[child.name], root));
				}
			}
        }

        private PathfinderRigidTransform CreatePathfinderTransform(Transform child, string path, Transform root)
        {
	        var pathfinderTransform = new GameObject($"Pathfinder-{child.name}").AddComponent<PathfinderRigidTransform>();
			pathfinderTransform.transform.SetParent(child, false);
	        pathfinderTransform.Root = root;
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

	        var result = SDKBridgePathfinder.CopyPath(globalPathBase, localPathBase);
	        SDKBridgePathfinder.GetValue<SDKRigidTransform>($"{localPathBase}.bob.stage.avatar.trackers.leftFoot", out var localValue, (int) PathfinderType.RigidTransform);
	        Debug.Log($"localValue {localValue.pos.z}");
	        
	        SDKBridgePathfinder.GetValue<SDKRigidTransform>($"{globalPathBase}.bob.stage.avatar.trackers.leftFoot", out var globalValue, (int) PathfinderType.RigidTransform);
	        Debug.Log($"globalValue {globalValue.pos.z}");
        }
    }
}