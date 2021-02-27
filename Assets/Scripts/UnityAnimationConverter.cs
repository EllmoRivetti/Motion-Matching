using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace MotionMatching.Animation
{
    [RequireComponent(typeof(Animator))]
    public class UnityAnimationConverter : MonoBehaviour
    {
        [ShowInInspector] public LoadedBonesMatching m_BonesMatching;
        public LoadedAnimationFile file;
        public Animator playerAnimator;
        private AnimatorClipInfo[] animationClip;

        public float m_AnimationSpeedMultiplicator;
        private float m_AnimationSpeed;
        private float m_AnimatorInitialSpeed;

        private bool m_Busy = false;
        private int m_MaxFrame;

        void OnValidate()
        {
            m_AnimationSpeed = m_AnimatorInitialSpeed * m_AnimationSpeedMultiplicator;
        }
        
        void Start()
        {
            animationClip = playerAnimator.GetCurrentAnimatorClipInfo(0);
            m_AnimatorInitialSpeed = playerAnimator.speed;
            m_AnimationSpeed = m_AnimatorInitialSpeed * m_AnimationSpeedMultiplicator;
            playerAnimator.speed = 0;
        }
        public void Create(string name)
        {
            file = ScriptableObject.CreateInstance<LoadedAnimationFile>();

            AssetDatabase.CreateAsset(file, name);
            AssetDatabase.SaveAssets();

            LoadAnimationFromUnityAnimator();
        }
        public bool CanRetrieveLoadedAnimationFile()
        {
            return !m_Busy;
        }
        [Button]
        public void LoadAnimationFromUnityAnimator()
        {
            playerAnimator.Play(animationClip[0].clip.name, -1, 0f);
            file.m_FrameData = new SortedDictionary<int, Dictionary<RigBodyParts, BoneData>>();
            playerAnimator.speed = m_AnimationSpeed * m_AnimationSpeedMultiplicator;

            int totalFrames = (int)(animationClip[0].weight * (animationClip[0].clip.length * animationClip[0].clip.frameRate));
            file.m_FrameRate = (int) animationClip[0].clip.frameRate;
            // StartCreation(totalFrames);
            for (int i_frame = 0; i_frame <= totalFrames; ++i_frame)
            {
                AnimationEvent animationEvent = new AnimationEvent();
                animationEvent.functionName = "AddFrame";
                animationEvent.floatParameter = i_frame;
                animationEvent.time = ((i_frame * 1.0f) / (totalFrames * 1.0f)) * animationClip[0].clip.length;
                animationClip[0].clip.AddEvent(animationEvent);
            }
        }

        public void AddFrame(float frame)
        {
            AddFrameDataToLoadedAnimationFile(file, (int)frame);
            // StartCoroutine(NextFrame(frame));
        }

        public void AddFrameDataToLoadedAnimationFile(LoadedAnimationFile file, int frame)
        {
            // Stop animation
            playerAnimator.speed = 0;

            Dictionary<RigBodyParts, BoneData> currentFrameBoneData = new Dictionary<RigBodyParts, BoneData>();
            foreach (var kvp in m_BonesMatching.m_Bones)
            {
                var boneName = kvp.Key;
                var boneTransform = kvp.Value;

                if (boneTransform != null)
                {
                    Vector3 position_ls, eulerAngles_d, localScale;
                    Quaternion rotation;

                    if (boneName == RigBodyParts.hip)
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
                        m_Position_ls    = position_ls,
                        m_EulerAngles_ls_d = eulerAngles_d,
                        m_LocalScale  = localScale,
                        m_Rotation = rotation,
                        m_Forward = boneTransform.forward
                    };

                    currentFrameBoneData.Add(boneName, singleBoneData);
                }
            }
            if (file.m_FrameData.ContainsKey(frame))
                file.m_FrameData.Remove(frame);
            file.m_FrameData.Add(frame, currentFrameBoneData);
            // Resume animation
            playerAnimator.speed = m_AnimationSpeed;
            // TryStopCreation(frame);
        }

        public void PrintEvent(string s)
        {
            Debug.Log("PrintEvent: " + s + " called at: " + Time.time);
        }
    }

}