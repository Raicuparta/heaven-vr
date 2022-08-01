using BoneworksLIV.AvatarTrackers;
using LIV.SDK.Unity;
using UnityEngine;

namespace LIV.AvatarTrackers
{
    public struct SDKRigidTransform
    {
        public SDKVector3 pos;
        public SDKQuaternion rot;
    }
    
    public class PathfinderRigidTransform: MonoBehaviour
    {
        public string Key;
        public string PathBase;
        public Transform Root;

        private SDKRigidTransform rigidTransform;
        private string path;

        private void Start()
        {
            path = $"{PathBase}.{Key}";
        }

        public void SetPathfinderValuesLocally()
        {
            if (!Root) return;
            
            rigidTransform.pos = Root.InverseTransformPoint(transform.position);
            rigidTransform.rot = Quaternion.Inverse(Root.rotation) * transform.rotation;

            SDKBridgePathfinder.SetValue(path, ref rigidTransform, (int) PathfinderType.RigidTransform);
        }
    }
}