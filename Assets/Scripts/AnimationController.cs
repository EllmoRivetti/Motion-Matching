using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MotionMatching.Animation
{
	public enum RigBodyParts
	{
		HIPS, SPINE, CHEST, UPPER_CHEST,
		LEFT_ARM_SHOULDER, LEFT_ARM_UPPER, LEFT_ARM_LOWER, LEFT_ARM_HAND,
		RIGHT_ARM_SHOULDER, RIGHT_ARM_UPPER, RIGHT_ARM_LOWER, RIGHT_ARM_HAND,
		LEFT_LEG_UPPER, LEFT_LEG_LOWER, LEFT_LEG_FOOT, LEFT_LEG_TOES,
		RIGHT_LEG_UPPER, RIGHT_LEG_LOWER, RIGHT_LEG_FOOT, RIGHT_LEG_TOES,

		HEAD_NECK, HEAD_HEAD, HEAD_LEFT_EYE, HEAD_RIGHT_EYE, HEAD_JAW,

		LEFT_HAND_THUMB_PROXIMAL, LEFT_HAND_THUMB_INTERMEDIATE, LEFT_HAND_THUMB_DISTAL,
		LEFT_HAND_INDEX_PROXIMAL, LEFT_HAND_INDEX_INTERMEDIATE, LEFT_HAND_INDEX_DISTAL,
		LEFT_HAND_MIDDLE_PROXIMAL, LEFT_HAND_MIDDLE_INTERMEDIATE, LEFT_HAND_MIDDLE_DISTAL,
		LEFT_HAND_RING_PROXIMAL, LEFT_HAND_RING_INTERMEDIATE, LEFT_HAND_RING_DISTAL,
		LEFT_HAND_LITTLE_PROXIMAL, LEFT_HAND_LITTLE_INTERMEDIATE, LEFT_HAND_LITTLE_DISTAL,

		RIGHT_HAND_THUMB_PROXIMAL, RIGHT_HAND_THUMB_INTERMEDIATE, RIGHT_HAND_THUMB_DISTAL,
		RIGHT_HAND_INDEX_PROXIMAL, RIGHT_HAND_INDEX_INTERMEDIATE, RIGHT_HAND_INDEX_DISTAL,
		RIGHT_HAND_MIDDLE_PROXIMAL, RIGHT_HAND_MIDDLE_INTERMEDIATE, RIGHT_HAND_MIDDLE_DISTAL,
		RIGHT_HAND_RING_PROXIMAL, RIGHT_HAND_RING_INTERMEDIATE, RIGHT_HAND_RING_DISTAL,
		RIGHT_HAND_LITTLE_PROXIMAL, RIGHT_HAND_LITTLE_INTERMEDIATE, RIGHT_HAND_LITTLE_DISTAL
	}
	public struct BoneData
	{
		public Vector3 m_Position;
		public Vector3 m_Rotation;
		public Vector3 m_Scale;
	}

	public class AnimationController : SerializedMonoBehaviour
	{
		[Header("Animation data")]
		[ShowInInspector, ReadOnly] public Dictionary<RigBodyParts, Transform> m_Bones;

		// frames number start at 1
		[ShowInInspector, ReadOnly] public SortedDictionary<int, Dictionary<RigBodyParts, BoneData>> m_FrameData; 

		[Header("Parameters")]
		public int m_FramesPerSecond = 30;

		protected int m_CurrentFrame = 1;
		protected int m_LastFrameNumber = -1;

		protected bool m_Run = false;
		protected bool m_Pause = false;

		#region Event buttons
		[Button]
		public void Run()
		{
			m_Run = true;
		}
		[Button]
		public void Stop()
		{
			m_Run = false;
		}
		[Button]
		public void Pause()
		{
			m_Pause = !m_Pause;
		}
		#endregion

		public IEnumerator RunAnimation()
		{
			while(m_Run && !m_Pause)
			{
				var currentFrameData = GetBonesDataForFrame(m_CurrentFrame);
				SetBonesData(currentFrameData);

				yield return new WaitForSeconds(1 / m_FramesPerSecond);
				m_CurrentFrame++;
			}
		}

		public void SetBonesData(Dictionary<RigBodyParts, BoneData> currentFrameData)
		{
			foreach (var rigFrameData in currentFrameData)
			{
				var bone = m_Bones[rigFrameData.Key];
				var boneFrameData = rigFrameData.Value;

				SetBoneData(bone, boneFrameData);
			}
		}
		public void SetBoneData(Transform t, BoneData bd)
		{
			t.position = bd.m_Position;
			t.eulerAngles = bd.m_Rotation;
			t.localScale = bd.m_Scale;
		}

		/*
		 * Accepts only fbx file format
		 * Constructs the m_FrameData structure and sets the m_LastFrameNumber
		 * m_FrameData has to be ordered by the int 
		 */
		public void ReadAnimation(string filename)
		{

		}


        public Dictionary<RigBodyParts, BoneData> GetBonesDataForFrame(int frameNb)
        {
            int firstFrameNb = -1,
                secondFrameNb = -1;

            Dictionary<RigBodyParts, BoneData> firstFrameBonesData  = new Dictionary<RigBodyParts, BoneData>(),
                                               secondFrameBonesData = new Dictionary<RigBodyParts, BoneData>();

            //Return value
            Dictionary<RigBodyParts, BoneData> bonesDataForFrame = new Dictionary<RigBodyParts, BoneData>(); 


            foreach (KeyValuePair<int, Dictionary<RigBodyParts, BoneData>> bonesData in m_FrameData)
            {
                if(bonesData.Key < frameNb)
                {
                    if(firstFrameNb == -1 && secondFrameNb == -1)
                    {
                        firstFrameNb = bonesData.Key;
                        firstFrameBonesData = bonesData.Value;

                        secondFrameNb = bonesData.Key;
                        secondFrameBonesData = bonesData.Value;
                    }
                    else
                    {
                        firstFrameNb = bonesData.Key;
                        firstFrameBonesData = bonesData.Value;
                    }
                }
                else if (bonesData.Key > frameNb)
                {
                    secondFrameNb = bonesData.Key;
                    secondFrameBonesData = bonesData.Value;
                    break;
                }
                else// frameNb == bonesData.key
                {
                    return bonesData.Value;
                }
            }

            foreach (KeyValuePair<RigBodyParts, BoneData> boneData in firstFrameBonesData)
            {
                BoneData data = GetBoneDataForFrameT(boneData.Value, secondFrameBonesData[boneData.Key], firstFrameNb, secondFrameNb);
                bonesDataForFrame.Add(boneData.Key, data);
            }

            return bonesDataForFrame;
        }

        public BoneData GetBoneDataForFrameT(BoneData firstFrameBoneData, BoneData secondFrameBoneData, float firstFrameNb, float secondFrameNb)
        {
            BoneData boneData = new BoneData();

            float t = firstFrameNb / secondFrameNb;

            boneData.m_Rotation = GetInterpolatedValue(firstFrameBoneData.m_Rotation, secondFrameBoneData.m_Rotation, t);
            boneData.m_Position = GetInterpolatedValue(firstFrameBoneData.m_Position, secondFrameBoneData.m_Position, t);
			boneData.m_Scale	= GetInterpolatedValue(firstFrameBoneData.m_Scale, secondFrameBoneData.m_Scale, t);

			return boneData;
        }

        public Vector3 GetInterpolatedValue(Vector3 a, Vector3 b, float t)
        {
            return Vector3.Lerp(a, b, t);
        }
    }
}
