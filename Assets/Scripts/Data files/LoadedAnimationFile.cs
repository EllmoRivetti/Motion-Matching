using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MotionMatching.Animation
{
    [CreateAssetMenu(fileName = "MotionMatching / LoadedAnimation", menuName = "MotionMatching/LoadedAnimation", order = 1)]
	public class LoadedAnimationFile : SerializedScriptableObject
	{
		public SortedDictionary<int, Dictionary<eRigBodyParts, BoneData>> m_FrameData;
		public int m_FrameRate;
	}
}
