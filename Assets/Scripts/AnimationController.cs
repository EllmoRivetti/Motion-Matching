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
        [ShowInInspector] public Dictionary<eRigBodyParts, Transform> m_RigidbodyPartToTransform;

        public LoadedBonesMatching m_LoadedBonesMatching;
        public LoadedMocapFrameData m_LoadedMocapFrameData;

        // frames number start at 1
        public LoadedAnimationFile m_LoadedAnimationFile;

        [Header("Parameters")]
        [ShowInInspector] protected int m_CurrentFrame = 1;
        protected int m_LastFrameNumber = -1;
        [ReadOnly] public int m_FramesToRun = -1; // If set to -1, this paramter is ignored
        private int m_FramesRate = 30;

        [Header("Status")]
        [ShowInInspector, ReadOnly] protected bool m_IsRunning = false;
        [ShowInInspector, ReadOnly] protected bool m_IsPaused = false;

        protected IEnumerator m_AnimationCoroutine;
        protected bool m_AnimationCoroutine_isRunning = false;

        private event Action m_OnAnimationEnd;

        #endregion

        #region Helper methods
        // This method only exists so we can copy m_RigidbodyPartToTransform to a file in order to use it elsewhere
        [Button]
        public void CopyBonesMatching()
        {
            m_LoadedBonesMatching.m_Bones = m_RigidbodyPartToTransform;
        }

        // https://forum.unity.com/threads/transform-inversetransformpoint-without-transform.954939/
        // https://twitter.com/georgerrmartin_/status/410279960624918529?lang=fr
        Vector3 InverseTransformPoint(Vector3 transforPos, Quaternion transformRotation, Vector3 transformScale, Vector3 pos)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transforPos, transformRotation, transformScale);
            Matrix4x4 inverse = matrix.inverse;
            return inverse.MultiplyPoint3x4(pos);
        }
        #endregion
        #region Unity events
        private void OnValidate()
        {
            //  Bind bones
            if (m_RigidbodyPartToTransform == null)
            {
                m_RigidbodyPartToTransform = new Dictionary<eRigBodyParts, Transform>();
                foreach (eRigBodyParts value in Enum.GetValues(typeof(eRigBodyParts)))
                {
                    m_RigidbodyPartToTransform[value] = null;
                }
            }
            m_FramesRate = m_LoadedAnimationFile.m_FrameRate;
        }

        public void Start()
        {
            CopyBonesMatching();
            //print("Successfully copied bones matching from controller");

            InitMocapFrameData();
            //print("Successfully initiated motion capture frame data");
        }
        #endregion
        
        #region Event buttons
        [Button]
        public void Run()
        {
            if (!m_AnimationCoroutine_isRunning)
            {
                m_IsRunning = true;
                m_AnimationCoroutine = RunAnimation();
                StartCoroutine(m_AnimationCoroutine);
                m_AnimationCoroutine_isRunning = true;
            }
        }

        [Button]
        public void RunFromFrame(int i_frame)
        {
            //print("RunFromFrame(" + i_frame + ")");
            m_CurrentFrame = i_frame;
            Run();
        }
        [Button]
        public void RunNFramesFromFrame(int n, int i_frame, Action onFinishedAnimationSequence = null)
        {
            //print("RunNFramesFromFrame(" + n + ", " + i_frame + ", " + onFinishedAnimationSequence == null + ")");
            m_FramesToRun = n;
            RunFromFrame(i_frame);
            m_OnAnimationEnd += onFinishedAnimationSequence;
            m_OnAnimationEnd += () => m_FramesToRun = -1;
        }

        [Button]
        public void Stop()
        {
            try
            {
                m_IsRunning = false;
                StopCoroutine(m_AnimationCoroutine);
                m_AnimationCoroutine = null;
                m_AnimationCoroutine_isRunning = false;
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
            m_IsPaused = !m_IsPaused;
        }
        #endregion
        #region Running Animation
        // This is the main method that runs an animation
        private IEnumerator RunAnimation()
        {
            if (!Application.isPlaying) yield return null;

            //print("Running animation");
            while (
                m_IsRunning &&
                !m_IsPaused &&
                m_CurrentFrame <= m_LoadedAnimationFile.m_FrameData.Count &&
                (m_FramesToRun == -1 || m_FramesToRun > 0) // parameter m_FramesToRun is not set OR parameter m_FramesToRun is set
            )
            {
                if (!m_IsPaused)
                {
                    // Apply current frame data to the rig
                    var currentFrameData = GetBonesDataForFrame(m_CurrentFrame);
                    SetBonesData(currentFrameData);

                    // print("m_CurrentFrame: " + m_CurrentFrame);

                    // Wait next frame
                    yield return new WaitForSeconds(1.0f / (float)m_FramesRate);

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
            m_AnimationCoroutine_isRunning = false;
            //print("Finished animation");


            // Run onAnimationEnd event and clear it from all subscribers
            if (m_OnAnimationEnd != null)
            {
                m_OnAnimationEnd();
                m_OnAnimationEnd = null;
            }
        }
        #endregion
        #region FrameDataCreation
        // For all frames, init their motion matching data
        [Button]
        public void InitMocapFrameData()
        {
            if (m_LoadedMocapFrameData == null)
            {
                Debug.LogError("Please set m_LoadedMocapFrameData in AnimationController", this);
                return;
            }

            m_LoadedMocapFrameData.m_FrameData = new SortedDictionary<int, MocapFrameData>();

            for (int i_frame = 0; i_frame < m_LoadedAnimationFile.m_FrameData.Count - MotionMatching.Constants.MM_NEXT_FRAME_INTERVAL_SIZE; ++i_frame)
            {
                var frameData = CreateMocapFrameData(i_frame);
                m_LoadedMocapFrameData.m_FrameData.Add(i_frame, frameData);
            }
        }
        MocapFrameData CreateMocapFrameData(int i_frame)
        {
            Vector3 positionHipProjection_l = m_LoadedAnimationFile.m_FrameData[i_frame][eRigBodyParts.hip].m_Position_l;
            Vector3 positionFuturHipProjection_l = m_LoadedAnimationFile.m_FrameData[i_frame + MotionMatching.Constants.MM_NEXT_FRAME_INTERVAL_SIZE][eRigBodyParts.hip].m_Position_l;

            Vector3 positionRFeet_l = m_LoadedAnimationFile.m_FrameData[i_frame][eRigBodyParts.rFoot].m_Position_l;
            Vector3 positionLFeet_l = m_LoadedAnimationFile.m_FrameData[i_frame][eRigBodyParts.lFoot].m_Position_l;

            Vector3 rightFeetPositionProjectedInHipSystem_l = InverseTransformPoint(positionHipProjection_l, Quaternion.identity, Vector3.one, positionRFeet_l),
                    leftFeetPositionProjectedInHipSystem_l = InverseTransformPoint(positionHipProjection_l, Quaternion.identity, Vector3.one, positionLFeet_l);

            MocapFrameData.FeetPositions positionFeet_l = new MocapFrameData.FeetPositions
            {
                m_PositionRightFoot_l = rightFeetPositionProjectedInHipSystem_l,
                m_PositionLeftFoot_l = leftFeetPositionProjectedInHipSystem_l
            };

            return new MocapFrameData(
                i_frame,
                positionHipProjection_l,
                positionFuturHipProjection_l,
                positionFeet_l,
                m_LoadedAnimationFile.m_FrameData[i_frame][eRigBodyParts.hip].m_EulerAngles_l_d,
                m_LoadedAnimationFile.m_FrameData[i_frame][eRigBodyParts.hip].m_Rotation_q,
                m_LoadedAnimationFile.m_FrameData[i_frame][eRigBodyParts.hip].m_Forward
            );
        }
        #endregion
        #region SetOrGetBones
        public void SetBonesData(Dictionary<eRigBodyParts, BoneData> currentFrameData)
        {
            foreach (var kvpBone in m_RigidbodyPartToTransform)
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
            t.localPosition = bd.m_Position_l;
            t.localRotation = bd.m_Rotation_q;
            t.localScale = bd.m_Scale_l;
        }
        public void SetBoneData(Transform t, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            t.position = position;
            t.eulerAngles = eulerAngles;
            t.localScale = scale;
        }

        public Dictionary<eRigBodyParts, BoneData> GetBonesDataForFrame(int frameNb)
        {
            if (m_LoadedAnimationFile.m_FrameData == null) return null;

            // print("GetBonesDataForFrame: " + frameNb);
            // print(m_AnimationToUse.m_FrameData.ContainsKey(frameNb));
            //Return value
            Dictionary<eRigBodyParts, BoneData> interpolatedFrame = new Dictionary<eRigBodyParts, BoneData>();
            int firstFrame = -1, nextFrame = -1;

            foreach (int i in m_LoadedAnimationFile.m_FrameData.Keys)
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
                    return m_LoadedAnimationFile.m_FrameData[i];
                }
            }

            // Debug.Log ("Searching:" + frameNb);
            // print("Found: " + firstFrame);
            // print("Next: " + nextFrame);

            var firstFrameData = m_LoadedAnimationFile.m_FrameData[firstFrame];
            var nextFrameData = m_LoadedAnimationFile.m_FrameData[nextFrame];

            if (firstFrameData == null || nextFrameData == null)
                Debug.LogError("Something went wrong in GetBonesDataForFrame", this);

            foreach (var currentBoneData in m_LoadedAnimationFile.m_FrameData[firstFrame])
            {
                eRigBodyParts bodyPartName = currentBoneData.Key;
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

            boneData.m_EulerAngles_l_d = GetInterpolatedValue(firstFrameBoneData.m_EulerAngles_l_d, secondFrameBoneData.m_EulerAngles_l_d, t);
            boneData.m_Position_l = GetInterpolatedValue(firstFrameBoneData.m_Position_l, secondFrameBoneData.m_Position_l, t);
            boneData.m_Scale_l = GetInterpolatedValue(firstFrameBoneData.m_Scale_l, secondFrameBoneData.m_Scale_l, t);

            return boneData;
        }
        public Vector3 GetInterpolatedValue(Vector3 a, Vector3 b, float t)
        {
            return Vector3.Lerp(a, b, t);
        }
        #endregion
    }
}
