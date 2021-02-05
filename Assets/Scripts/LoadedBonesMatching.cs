using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MotionMatching.Animation
{
    [CreateAssetMenu(fileName = "MotionMatching / BonesMatching", menuName = "MotionMatching / BonesMatching", order = 2)]
    public class LoadedBonesMatching : SerializedScriptableObject
    {
		public Dictionary<RigBodyParts, Transform> m_Bones;
	}
}
