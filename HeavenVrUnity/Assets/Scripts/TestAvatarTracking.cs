using System.Collections.Generic;
using LIV.AvatarTrackers;
using UnityEngine;

public class TestAvatarTracking : MonoBehaviour
{
	[SerializeField]
	private Transform root;
	
	private static readonly Dictionary<string, string> boneMap = new Dictionary<string, string>()
	{
		// { "B-head", "bob.stage.avatar.trackers.head" }, // other options: Neck_01SHJnt / Neck_02SHJnt / Neck_TopSHJnt / Head_JawSHJnt / Head_TopSHJnt
		// { "B-upperChest", "bob.stage.avatar.trackers.chest" }, // TODO: chest looks hella broken, better off not tracking it at all.
		// { "B-hand.L", "bob.stage.avatar.trackers.leftHand" }, // other options:  l_Hand_1SHJnt / l_Hand_2SHJnt / l_GripPoint_AuxSHJnt
		// { "B-forearm.L", "bob.stage.avatar.trackers.leftElbowGoal" },
		// { "B-hand.R", "bob.stage.avatar.trackers.rightHand" }, // other options: r_Hand_1SHJnt / r_Hand_2SHJnt / r_GripPoint_AuxSHJnt
		// { "B-spine", "bob.stage.avatar.trackers.waist" },
		{ "B-forearm.R", "bob.stage.avatar.trackers.rightElbowGoal" },
		{ "B-toe.L", "bob.stage.avatar.trackers.leftFoot" }, // other options:  l_Leg_BallSHJnt
		{ "B-shin.L", "bob.stage.avatar.trackers.leftKneeGoal" },
		{ "B-toe.R", "bob.stage.avatar.trackers.rightFoot" }, // other options: r_Leg_BallSHJnt
		{ "B-shin.R", "bob.stage.avatar.trackers.rightKneeGoal" },
	};
    
    void Start()
    {
	    var avatarTrackers = gameObject.AddComponent<PathfinderAvatarTrackers>();
	    avatarTrackers.SetUp(root, boneMap);
    }
}
