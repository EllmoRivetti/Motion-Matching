using System;
using UnityEngine;

namespace MotionMatching.Animation
{
    [Serializable]
	public struct BoneData
	{
		public Vector3 m_Position_l;
		public Vector3 m_EulerAngles_l_d;
		public Vector3 m_Forward;
		public Quaternion m_Rotation_q;
		public Vector3 m_Scale_l;
	}
}
