using System;
using UnityEngine;

namespace MotionMatching.Animation
{
    [Serializable]
	public struct BoneData
	{
		public Vector3 m_Position_ws;
		public Vector3 m_EulerAngles_d;
		public Vector3 m_LocalScale;
	}
}
