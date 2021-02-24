using System;
using UnityEngine;

namespace MotionMatching.Animation
{
    [Serializable]
	public struct BoneData
	{
		public Vector3 m_Position_ls;
		public Vector3 m_EulerAngles_ls_d;
		public Vector3 m_Forward;
		public Quaternion m_Rotation;
		public Vector3 m_LocalScale;
	}
}
