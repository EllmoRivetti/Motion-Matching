using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MotionMatching.Matching;

namespace MotionMatching.Animation
{

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
		public LoadedAnimationFile m_AnimationToUse;

		[Header("Parameters")]
		public int m_FramesPerSecond = 30;

		[ShowInInspector] protected int m_CurrentFrame = 1;
		protected int m_LastFrameNumber = -1;

		[Header("Status")]
		[ShowInInspector, ReadOnly] protected bool m_Run = false;
		[ShowInInspector, ReadOnly] protected bool m_Pause = false;

		protected IEnumerator m_AnimationCR;
		protected bool m_AnimationCR_isRunning = false;

		public int m_FramesToRun = -1; // If set to -1, this paramter is discarded
		private event Action onAnimationEnd;

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
		}

        public void Start()
        {
			CopyBonesMatching();
			print("Successfully copied bones matching from controller");

			InitMocapFrameData();
			print("Successfully initiated motion capture frame data");
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
		public void RunFromFrame(int i_frame)
        {
			print("RunFromFrame(" + i_frame + ")");
			m_CurrentFrame = i_frame;
			Run();
		}
		[Button]
		public void RunNFramesFromFrame(int n, int i_frame, Action onFinishedAnimationSequence = null)
		{
			print("RunNFramesFromFrame(" + n + ", " + i_frame + ", " + onFinishedAnimationSequence == null + ")");
			m_FramesToRun = n;
			RunFromFrame(i_frame);
			onAnimationEnd += onFinishedAnimationSequence;
			onAnimationEnd += () => m_FramesToRun = -1;
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

		private IEnumerator RunAnimation()
		{
			if (!Application.isPlaying) yield return null;

			print("Running animation");
			while (
				m_Run && 
				!m_Pause && 
				m_CurrentFrame <= m_AnimationToUse.m_FrameData.Count && 
				(m_FramesToRun == -1 || m_FramesToRun > 0) // parameter m_FramesToRun is not set OR parameter m_FramesToRun is set
			)
			{
				if (!m_Pause)
				{
					// Apply current frame data to the rig
					var currentFrameData = GetBonesDataForFrame(m_CurrentFrame);
					SetBonesData(currentFrameData);

					// print("m_CurrentFrame: " + m_CurrentFrame);

					// Wait next frame
					yield return new WaitForSeconds(1.0f / (float)m_FramesPerSecond);
					
					// Change current frame indexes
					m_CurrentFrame++;
					if (m_FramesToRun != -1)
						m_FramesToRun--;
				}
				else
				{
					yield return new WaitForSeconds(.1f);
				}

			}
			m_AnimationCR_isRunning = false;
			print("Finished animation");


			// Run onAnimationEnd event and clear it from all subscribers
			if (onAnimationEnd != null)
            {
				onAnimationEnd();
				onAnimationEnd = null;
            }
		}


        #region FrameDataCreation
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
			// noter la TRS_w des hips courante
			// faire avancer l'anim de 50 frames
			// noter la TRS_w des hips du futur
			// changer de repere les hips du futur pour les mettre dans les hips courante

			// hipsFutur_TRS_w[derniere colonne] => posHipFuture_w
			// inverser(hipCourante_TRS_w) * posHipFuture_w => posHipFutures_hipCourante
			// garder juste le (X, Z)
			// normalizer le reste

			if (m_LoadedMocapFrameData == null)
            {
				Debug.LogError("Please set m_LoadedMocapFrameData in AnimationController", this);
				return;
            }
				
			m_LoadedMocapFrameData.m_FrameData = new SortedDictionary<int, MocapFrameData>();

			for (int i_frame = 0; i_frame < m_AnimationToUse.m_FrameData.Count - MotionMatching.Constants.MM_NEXT_FRAME_INTERVAL_SIZE; ++i_frame)
			{
				var frameData = CreateDataFromFrame(i_frame);
				m_LoadedMocapFrameData.m_FrameData.Add(i_frame, frameData);
			}
		}
		MocapFrameData CreateDataFromFrame(int i_frame)
        {
			Vector3 positionHipProjection = m_AnimationToUse.m_FrameData[i_frame][RigBodyParts.hip].m_Position_ls;
			Vector3 positionFuturHipProjection = m_AnimationToUse.m_FrameData[i_frame + MotionMatching.Constants.MM_NEXT_FRAME_INTERVAL_SIZE][RigBodyParts.hip].m_Position_ls;

			Vector3 positionRFeet = m_AnimationToUse.m_FrameData[i_frame][RigBodyParts.rFoot].m_Position_ls;
			Vector3 positionLFeet = m_AnimationToUse.m_FrameData[i_frame][RigBodyParts.lFoot].m_Position_ls;

			Vector3 rightFeetPositionProjectedInHipSystem = InverseTransformPoint(positionHipProjection, Quaternion.identity, Vector3.one, positionRFeet),
					leftFeetPositionProjectedInHipSystem = InverseTransformPoint(positionHipProjection, Quaternion.identity, Vector3.one, positionLFeet);

			MocapFrameData.FeetPositions positionFeet = new MocapFrameData.FeetPositions
			{
				m_PositionRightFoot = rightFeetPositionProjectedInHipSystem,
				m_PositionLeftFoot = leftFeetPositionProjectedInHipSystem
			};

			return new MocapFrameData(
				i_frame,
				positionHipProjection, // new Vector2(positionHipProjection.x, positionHipProjection.z),
				positionFuturHipProjection, // new Vector2(positionFuturHipProjection.x, positionFuturHipProjection.z),
				positionFeet,
				m_AnimationToUse.m_FrameData[i_frame][RigBodyParts.hip].m_EulerAngles_ls_d,
				m_AnimationToUse.m_FrameData[i_frame][RigBodyParts.hip].m_Rotation,
				m_AnimationToUse.m_FrameData[i_frame][RigBodyParts.hip].m_Forward
			);
		}
		#endregion

		public void FixDefaultPosition()
        {
			var firstFrame = m_AnimationToUse.m_FrameData[1];
			for(int i = 2; i < m_AnimationToUse.m_FrameData.Count; ++i)
            {
				if (m_AnimationToUse.m_FrameData.ContainsKey(i))
                {
					var currentFrame = m_AnimationToUse.m_FrameData[i];
					List<RigBodyParts> keys = new List<RigBodyParts>(currentFrame.Keys);
					foreach (RigBodyParts currentBoneName in keys)
                    {
						BoneData currentBoneObject = currentFrame[currentBoneName];
						BoneData firstFrameBoneObject = firstFrame[currentBoneName];

						currentBoneObject.m_Position_ls = (currentBoneObject.m_Position_ls - firstFrameBoneObject.m_Position_ls) * .1f;
						m_AnimationToUse.m_FrameData[i][currentBoneName] = currentBoneObject;
						// currentBoneObject.m_Rotation -= firstFrameBoneObject.m_Rotation;
						// currentBoneObject.m_Scale -= firstFrameBoneObject.m_Scale;


					}
				}
            }
        }

		#region SetOrGetBones
		public void SetBonesData(Dictionary<RigBodyParts, BoneData> currentFrameData)
		{
			foreach (var kvpBone in m_Bones)
			{
				var bone = kvpBone.Value;
				var boneType = kvpBone.Key;
				if (bone != null)
				{
					var boneFrameData = currentFrameData[boneType];
					SetBoneData(bone, boneFrameData);
				}
			}
		}
		public void SetBoneData(Transform t, BoneData bd)
		{
			t.localPosition = bd.m_Position_ls;
			t.localRotation = bd.m_Rotation;
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
			if (m_AnimationToUse.m_FrameData == null) return null;

			// print("GetBonesDataForFrame: " + frameNb);
			// print(m_AnimationToUse.m_FrameData.ContainsKey(frameNb));
			//Return value
			Dictionary <RigBodyParts, BoneData> interpolatedFrame = new Dictionary<RigBodyParts, BoneData>();
			int firstFrame = -1, nextFrame = -1;

			foreach (int i in m_AnimationToUse.m_FrameData.Keys)
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
					return m_AnimationToUse.m_FrameData[i];
				}
			}

			// Debug.Log ("Searching:" + frameNb);
			// print("Found: " + firstFrame);
			// print("Next: " + nextFrame);

			var firstFrameData = m_AnimationToUse.m_FrameData[firstFrame];
			var nextFrameData = m_AnimationToUse.m_FrameData[nextFrame];

			if (firstFrameData == null || nextFrameData == null)
				Debug.LogError("Something went wrong in GetBonesDataForFrame", this);

			foreach (var currentBoneData in m_AnimationToUse.m_FrameData[firstFrame])
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

            boneData.m_EulerAngles_ls_d = GetInterpolatedValue(firstFrameBoneData.m_EulerAngles_ls_d, secondFrameBoneData.m_EulerAngles_ls_d, t);
            boneData.m_Position_ls = GetInterpolatedValue(firstFrameBoneData.m_Position_ls, secondFrameBoneData.m_Position_ls, t);
			boneData.m_LocalScale	= GetInterpolatedValue(firstFrameBoneData.m_LocalScale, secondFrameBoneData.m_LocalScale, t);

			return boneData;
        }
        public Vector3 GetInterpolatedValue(Vector3 a, Vector3 b, float t)
        {
            return Vector3.Lerp(a, b, t);
        }
		#endregion
	}
}
