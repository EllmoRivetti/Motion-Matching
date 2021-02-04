using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace MotionMatching.Animation
{
    [CreateAssetMenu(fileName = "MotionMatching / LoadedAnimation", menuName = "MotionMatching/LoadedAnimation", order = 1)]
	public class LoadedAnimationFile : SerializedScriptableObject
	{
		public SortedDictionary<int, Dictionary<RigBodyParts, BoneData>> m_FrameData;
		public int m_FrameRate;
	}
}
