using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MotionMatching.Matching;

namespace MotionMatching.Animation
{
	//public enum RigBodyParts
	//{
	//	HIPS, SPINE, CHEST, UPPER_CHEST,
	//	LEFT_ARM_SHOULDER, LEFT_ARM_UPPER, LEFT_ARM_LOWER, LEFT_ARM_HAND,
	//	RIGHT_ARM_SHOULDER, RIGHT_ARM_UPPER, RIGHT_ARM_LOWER, RIGHT_ARM_HAND,
	//	LEFT_LEG_UPPER, LEFT_LEG_LOWER, LEFT_LEG_FOOT, LEFT_LEG_TOES,
	//	RIGHT_LEG_UPPER, RIGHT_LEG_LOWER, RIGHT_LEG_FOOT, RIGHT_LEG_TOES,

	//	HEAD_NECK, HEAD_HEAD, HEAD_LEFT_EYE, HEAD_RIGHT_EYE, HEAD_JAW,

	//	LEFT_HAND_THUMB_PROXIMAL, LEFT_HAND_THUMB_INTERMEDIATE, LEFT_HAND_THUMB_DISTAL,
	//	LEFT_HAND_INDEX_PROXIMAL, LEFT_HAND_INDEX_INTERMEDIATE, LEFT_HAND_INDEX_DISTAL,
	//	LEFT_HAND_MIDDLE_PROXIMAL, LEFT_HAND_MIDDLE_INTERMEDIATE, LEFT_HAND_MIDDLE_DISTAL,
	//	LEFT_HAND_RING_PROXIMAL, LEFT_HAND_RING_INTERMEDIATE, LEFT_HAND_RING_DISTAL,
	//	LEFT_HAND_LITTLE_PROXIMAL, LEFT_HAND_LITTLE_INTERMEDIATE, LEFT_HAND_LITTLE_DISTAL,

	//	RIGHT_HAND_THUMB_PROXIMAL, RIGHT_HAND_THUMB_INTERMEDIATE, RIGHT_HAND_THUMB_DISTAL,
	//	RIGHT_HAND_INDEX_PROXIMAL, RIGHT_HAND_INDEX_INTERMEDIATE, RIGHT_HAND_INDEX_DISTAL,
	//	RIGHT_HAND_MIDDLE_PROXIMAL, RIGHT_HAND_MIDDLE_INTERMEDIATE, RIGHT_HAND_MIDDLE_DISTAL,
	//	RIGHT_HAND_RING_PROXIMAL, RIGHT_HAND_RING_INTERMEDIATE, RIGHT_HAND_RING_DISTAL,
	//	RIGHT_HAND_LITTLE_PROXIMAL, RIGHT_HAND_LITTLE_INTERMEDIATE, RIGHT_HAND_LITTLE_DISTAL
	//}
	[Serializable]
	public struct BoneData
	{
		public Vector3 m_Position;
		public Vector3 m_EulerAngles;
		public Vector3 m_LocalScale;
	}

	public class AnimationController : SerializedMonoBehaviour
	{
		#region Members
		[Header("Animation data")]
		[ShowInInspector] public Dictionary<RigBodyParts, Transform> m_Bones;

		public Transform m_CharacterToAnimate;
		public bool m_ApplyXZPositionTransformationToCharacter = false;
		public LoadedBonesMatching bonesMatching;
		public LoadedMocapFrameData m_LoadedMocapFrameData;

		// frames number start at 1
		[ShowInInspector, ReadOnly] public SortedDictionary<int, Dictionary<RigBodyParts, BoneData>> m_FrameData; 

		[Header("Parameters")]
		public int m_FramesPerSecond = 30;

		[ShowInInspector] protected int m_CurrentFrame = 1;
		protected int m_LastFrameNumber = -1;

		[Header("Status")]
		[ShowInInspector, ReadOnly] protected bool m_Run = false;
		[ShowInInspector, ReadOnly] protected bool m_Pause = false;

		protected IEnumerator m_AnimationCR;
		protected bool m_AnimationCR_isRunning = false;

		#endregion

		[Button]
		public void CopyBonesMatching()
        {
			bonesMatching.m_Bones = m_Bones;
		}

		private void OnValidate()
		{
			//  Bind bones
			if (m_Bones == null)
			{
				m_Bones = new Dictionary<RigBodyParts, Transform>();
				foreach (RigBodyParts value in Enum.GetValues(typeof(RigBodyParts)))
				{
					m_Bones[value] = null;
				}
			}
			// Swap frame
			if (Application.isPlaying)
            {
				var frameData = GetBonesDataForFrame(m_CurrentFrame);
				if (frameData != null)
					SetBonesData(frameData);
            }
		}

		#region Event buttons
		[Button]
		public void Run()
		{
			if (!m_AnimationCR_isRunning)
			{
				m_Run = true;
				m_AnimationCR = RunAnimation();
				StartCoroutine(m_AnimationCR);
				m_AnimationCR_isRunning = true;
			}
		}
		[Button]
		public void Stop()
		{
			try
			{
				m_Run = false;
				StopCoroutine(m_AnimationCR);
				m_AnimationCR = null;
				m_AnimationCR_isRunning = false;
				m_CurrentFrame = 0;
			}
			catch (UnityException e)
			{
				Debug.LogWarning(e.Message);
			}
		}
		[Button]
		public void Pause()
		{
			m_Pause = !m_Pause;
		}
		#endregion

		private void RemoveNullBonesFromMBones()
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("Please enter play mode to run this method!");
				return;
			}
			List<RigBodyParts> bones = new List<RigBodyParts>();
			foreach (var bone in m_Bones)
			{
				if (bone.Value == null)
				{
					bones.Add(bone.Key);
				}
			}
			bones.ForEach(x => m_Bones.Remove(x));
		}

		private IEnumerator RunAnimation()
		{
			if (!Application.isPlaying) yield return null;

			// RemoveNullBonesFromMBones();
			while (m_Run && !m_Pause && m_CurrentFrame <= m_FrameData.Count)
			{
				if (!m_Pause)
				{
					var currentFrameData = GetBonesDataForFrame(m_CurrentFrame);
					SetBonesData(currentFrameData);
					yield return new WaitForSeconds(1.0f / (float)m_FramesPerSecond);
					m_CurrentFrame++;
				}
				else
				{
					yield return new WaitForSeconds(.1f);
				}

			}
		}



		// https://forum.unity.com/threads/transform-inversetransformpoint-without-transform.954939/
		// https://twitter.com/georgerrmartin_/status/410279960624918529?lang=fr
		Vector3 InverseTransformPoint(Vector3 transforPos, Quaternion transformRotation, Vector3 transformScale, Vector3 pos)
		{
			Matrix4x4 matrix = Matrix4x4.TRS(transforPos, transformRotation, transformScale);
			Matrix4x4 inverse = matrix.inverse;
			return inverse.MultiplyPoint3x4(pos);
		}

		[Button]
		public void InitMocapFrameData()
		{
			if (m_LoadedMocapFrameData == null)
            {
				Debug.LogError("Please set m_LoadedMocapFrameData in AnimationController", this);
				return;
            }
				
			m_LoadedMocapFrameData.m_FrameData = new SortedDictionary<int, MocapFrameData>();
			for (int i_frame = 0; i_frame < m_FrameData.Count - 1; ++i_frame)
			{
				Vector3 positionHipProjection = m_FrameData[i_frame][RigBodyParts.hip].m_Position;
				Vector3 positionFuturHipProjection = m_FrameData[i_frame + 1][RigBodyParts.hip].m_Position;

				Vector3 positionRFeet = m_FrameData[i_frame][RigBodyParts.rFoot].m_Position;
				Vector3 positionLFeet = m_FrameData[i_frame][RigBodyParts.lFoot].m_Position;

				Vector3 rightFeetPositionProjectedInHipSystem = InverseTransformPoint(positionHipProjection, Quaternion.identity, Vector3.one, positionRFeet),
						leftFeetPositionProjectedInHipSystem = InverseTransformPoint(positionHipProjection, Quaternion.identity, Vector3.one, positionLFeet);

				MocapFrameData.FeetPositions positionFeet = new MocapFrameData.FeetPositions
				{
					m_PositionRightFoot = rightFeetPositionProjectedInHipSystem,
					m_PositionLeftFoot = leftFeetPositionProjectedInHipSystem
				};
				MocapFrameData frameData = new MocapFrameData(
					i_frame,
					positionHipProjection,
					positionFuturHipProjection,
					positionFeet
				);
				m_LoadedMocapFrameData.m_FrameData.Add(i_frame, frameData);
			}
			Debug.Log("Sucessfully initiated motion capture frame data.");
		}



		/*
		 * Accepts only fbx file format
		 * Constructs the m_FrameData structure and sets the m_LastFrameNumber
		 * m_FrameData has to be ordered by the int 
		 */
		public void BindAnimationData(SortedDictionary<int, Dictionary<RigBodyParts, BoneData>> animation)
		{
			// m_FrameData = new SortedDictionary<int, Dictionary<RigBodyParts, BoneData>>();
			// int limit = -1;
			// foreach (var kvp in animation)
			// {
			//  if (limit == -1) 
			// 	 	break;
			// 	m_FrameData.Add(kvp.Key, kvp.Value);
			//  limit--;
            // }
			m_FrameData = animation;
		}
		public void FixDefaultPosition()
        {
			var firstFrame = m_FrameData[1];
			for(int i = 2; i < m_FrameData.Count; ++i)
            {
				if (m_FrameData.ContainsKey(i))
                {
					var currentFrame = m_FrameData[i];
					List<RigBodyParts> keys = new List<RigBodyParts>(currentFrame.Keys);
					foreach (RigBodyParts currentBoneName in keys)
                    {
						BoneData currentBoneObject = currentFrame[currentBoneName];
						BoneData firstFrameBoneObject = firstFrame[currentBoneName];

						currentBoneObject.m_Position = (currentBoneObject.m_Position - firstFrameBoneObject.m_Position) * .1f;
						m_FrameData[i][currentBoneName] = currentBoneObject;
						// currentBoneObject.m_Rotation -= firstFrameBoneObject.m_Rotation;
						// currentBoneObject.m_Scale -= firstFrameBoneObject.m_Scale;


					}
				}
            }
        }

		#region SetOrGetBones
		public void SetBonesData(Dictionary<RigBodyParts, BoneData> currentFrameData)
		{
			Debug.Log("inside SetBonesData");
			foreach (var kvpBone in m_Bones)
			{
				var bone = kvpBone.Value;
				var boneType = kvpBone.Key;
				if (bone != null)
				{
					var boneFrameData = currentFrameData[boneType];
					SetBoneData(bone, boneFrameData);
                    // else
                    // {
					// 	Vector3 inverseHipMovement = -boneFrameData.m_Position;
					// 	SetBoneData(
					// 		bone,
					// 		boneFrameData.m_Position + inverseHipMovement,
					// 		boneFrameData.m_EulerAngles,
					// 		boneFrameData.m_LocalScale
					// 	);
					// }
				}
			}
			// foreach (var rigFrameData in currentFrameData)
			// {
			// 	var bone = m_Bones[rigFrameData.Key];
			// 	var boneFrameData = rigFrameData.Value;
			// 	if (bone == null)
            //     {
			// 		print("Cant set bonedata of " + rigFrameData.Key.ToString());
            //     }
            //     else
			// 	{
			// 		SetBoneData(bone, boneFrameData);
			// 	}
			// }
		}
		public void SetBoneData(Transform t, BoneData bd)
		{
			t.position= bd.m_Position;
			t.eulerAngles = bd.m_EulerAngles;
			t.localScale = bd.m_LocalScale;
		}
		public void SetBoneData(Transform t, Vector3 position, Vector3 eulerAngles, Vector3 scale)
		{
			t.position = position;
			t.eulerAngles = eulerAngles;
			t.localScale = scale;
		}

		public Dictionary<RigBodyParts, BoneData> GetBonesDataForFrame(int frameNb)
        {
			if (m_FrameData == null) return null;

			// print("GetBonesDataForFrame: " + frameNb);
			// print(m_FrameData.ContainsKey(frameNb));
			//Return value
			Dictionary <RigBodyParts, BoneData> interpolatedFrame = new Dictionary<RigBodyParts, BoneData>();
			int firstFrame = -1, nextFrame = -1;

			foreach (int i in m_FrameData.Keys)
			{
				if (i < frameNb)
				{
					if (firstFrame == -1 && nextFrame == -1)
					{
						firstFrame = i;
						nextFrame = i;
					}
					else
					{
						firstFrame = i;
					}
				}
				else if (i > frameNb)
				{
					nextFrame = i;
					break;
				}
				else// frameNb == bonesData.key
				{
					return m_FrameData[i];
				}
			}

			// Debug.Log ("Searching:" + frameNb);
			// print("Found: " + firstFrame);
			// print("Next: " + nextFrame);

			var firstFrameData = m_FrameData[firstFrame];
			var nextFrameData = m_FrameData[nextFrame];

			if (firstFrameData == null || nextFrameData == null)
				Debug.LogError("Something went wrong in GetBonesDataForFrame", this);

			foreach (var currentBoneData in m_FrameData[firstFrame])
			{
				RigBodyParts bodyPartName = currentBoneData.Key;
				BoneData interpolatedBodyPartData = GetBoneDataForFrameT(
					firstFrameData[bodyPartName],
					nextFrameData[bodyPartName],
					firstFrame,
					nextFrame
				);

				interpolatedFrame.Add(bodyPartName, interpolatedBodyPartData);
			}
			/*
			foreach (KeyValuePair<RigBodyParts, BoneData> boneData in firstFrameBonesData)
            {
                BoneData data = GetBoneDataForFrameT(boneData.Value, secondFrameBonesData[boneData.Key], firstFrameNb, secondFrameNb);
                bonesDataForFrame.Add(boneData.Key, data);
            }
			*/

			return interpolatedFrame;
        }
        public BoneData GetBoneDataForFrameT(BoneData firstFrameBoneData, BoneData secondFrameBoneData, float firstFrameNb, float secondFrameNb)
        {
            BoneData boneData = new BoneData();

            float t = firstFrameNb / secondFrameNb;

            boneData.m_EulerAngles = GetInterpolatedValue(firstFrameBoneData.m_EulerAngles, secondFrameBoneData.m_EulerAngles, t);
            boneData.m_Position = GetInterpolatedValue(firstFrameBoneData.m_Position, secondFrameBoneData.m_Position, t);
			boneData.m_LocalScale	= GetInterpolatedValue(firstFrameBoneData.m_LocalScale, secondFrameBoneData.m_LocalScale, t);

			return boneData;
        }
        public Vector3 GetInterpolatedValue(Vector3 a, Vector3 b, float t)
        {
            return Vector3.Lerp(a, b, t);
        }
		#endregion


		void OnGUI()
		{
			if (GUI.Button(new Rect(10, 10, 50, 50), "Run"))
				Run();

			if (GUI.Button(new Rect(10, 70, 50, 30), "Stop"))
				Stop();
		}
	}
}
