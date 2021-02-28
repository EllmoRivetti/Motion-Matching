using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MotionMatching.Animation
{
    [RequireComponent(typeof(Animator))]
    public class UnityAnimationConverter : MonoBehaviour
    {
        #region Members
        [ShowInInspector] public LoadedBonesMatching m_BonesMatching;
        public LoadedAnimationFile m_File;
        public float m_AnimationSpeedMultiplicator;

        Animator m_PlayerAnimator;
        float m_AnimationSpeed;
        float m_AnimatorInitialSpeed;
        AnimatorClipInfo[] m_AnimationClip;
        bool m_Busy = false;
        int m_MaxFrame;
        #endregion

        #region Unity events
        void OnValidate()
        {
            m_AnimationSpeed = m_AnimatorInitialSpeed * m_AnimationSpeedMultiplicator;
        }

        private void Awake()
        {
            m_PlayerAnimator = GetComponent<Animator>();
        }
        void Start()
        {
            m_AnimationClip = m_PlayerAnimator.GetCurrentAnimatorClipInfo(0);
            m_AnimatorInitialSpeed = m_PlayerAnimator.speed;
            m_AnimationSpeed = m_AnimatorInitialSpeed * m_AnimationSpeedMultiplicator;
            m_PlayerAnimator.speed = 0;
        }
        #endregion
        #region Load animation
        [Button]
        public void LoadAnimationFromUnityAnimator()
        {
            m_PlayerAnimator.Play(m_AnimationClip[0].clip.name, -1, 0f);
            m_File.m_FrameData = new SortedDictionary<int, Dictionary<eRigBodyParts, BoneData>>();
            m_PlayerAnimator.speed = m_AnimationSpeed * m_AnimationSpeedMultiplicator;

            int totalFrames = (int)(m_AnimationClip[0].weight * (m_AnimationClip[0].clip.length * m_AnimationClip[0].clip.frameRate));
            m_File.m_FrameRate = (int) m_AnimationClip[0].clip.frameRate;
            // StartCreation(totalFrames);
            for (int i_frame = 0; i_frame <= totalFrames; ++i_frame)
            {
                AnimationEvent animationEvent = new AnimationEvent();
                animationEvent.functionName = "AddFrame";
                animationEvent.floatParameter = i_frame;
                animationEvent.time = ((i_frame * 1.0f) / (totalFrames * 1.0f)) * m_AnimationClip[0].clip.length;
                m_AnimationClip[0].clip.AddEvent(animationEvent);
            }
        }
        public void AddFrame(float frame)
        {
            AddFrameDataToLoadedAnimationFile(m_File, (int)frame);
            // StartCoroutine(NextFrame(frame));
        }
        public void AddFrameDataToLoadedAnimationFile(LoadedAnimationFile file, int frame)
        {
            // Stop animation
            m_PlayerAnimator.speed = 0;

            Dictionary<eRigBodyParts, BoneData> currentFrameBoneData = new Dictionary<eRigBodyParts, BoneData>();
            foreach (var kvp in m_BonesMatching.m_Bones)
            {
                var boneName = kvp.Key;
                var boneTransform = kvp.Value;

                if (boneTransform != null)
                {
                    Vector3 position_ls, eulerAngles_d, localScale;
                    Quaternion rotation;

                    if (boneName == eRigBodyParts.hip)
                    {
                        position_ls = boneTransform.position;
                        rotation = boneTransform.rotation;
                    }
                    else
                    {
                        position_ls = boneTransform.localPosition;
                        rotation = boneTransform.localRotation;
                    }

                    eulerAngles_d = boneTransform.eulerAngles;
                    localScale = boneTransform.localScale;

                    BoneData singleBoneData = new BoneData
                    {
                        m_Position_l    = position_ls,
                        m_EulerAngles_l_d = eulerAngles_d,
                        m_Scale_l  = localScale,
                        m_Rotation_q = rotation,
                        m_Forward = boneTransform.forward
                    };

                    currentFrameBoneData.Add(boneName, singleBoneData);
                }
            }
            if (file.m_FrameData.ContainsKey(frame))
                file.m_FrameData.Remove(frame);
            file.m_FrameData.Add(frame, currentFrameBoneData);
            // Resume animation
            m_PlayerAnimator.speed = m_AnimationSpeed;
            // TryStopCreation(frame);
        }
        #endregion
    }

}