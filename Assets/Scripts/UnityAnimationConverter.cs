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
        private AnimatorStateInfo animationInfo;

        public float m_AnimationSpeedMultiplicator;
        private float m_AnimationSpeed;
        private float m_AnimatorInitialSpeed;

        private bool m_Busy = false;
        private int m_MaxFrame;

        void OnValidate()
        {
            m_AnimationSpeed = m_AnimatorInitialSpeed * m_AnimationSpeedMultiplicator;
        }

        // Start is called before the first frame update
        void Start()
        {
            animationInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
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

        private void StartCreation(int frameCount)
        {
            if (m_Busy)
                Debug.LogError("UnityAnimationConverter already busy!");
            m_Busy = true;
            m_MaxFrame = frameCount;
            playerAnimator.enabled = true;
        }

        private void TryStopCreation(int currentFrame)
        {
            m_Busy = (m_Busy && m_MaxFrame == currentFrame);
            playerAnimator.enabled = false;
        }

        [Button]
        public void LoadAnimationFromUnityAnimator()
        {
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


        //public SortedDictionary<int, Dictionary<RigBodyParts, BoneData>> m_FrameData;

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
                    BoneData singleBoneData = new BoneData
                    {
                        m_Position    = boneTransform.position,
                        m_EulerAngles = boneTransform.eulerAngles,
                        m_LocalScale  = boneTransform.localScale
                    };

                    currentFrameBoneData.Add(boneName, singleBoneData);
                }
            }
            file.m_FrameData.Add(frame, currentFrameBoneData);
            // Resume animation
            playerAnimator.speed = m_AnimationSpeed;
            // TryStopCreation(frame);
        }


        private IEnumerator NextFrame(float frame)
        {
            var speed = playerAnimator.speed;
            playerAnimator.speed = 0;
            print("Adding frame: " + frame);
            yield return waitForKeyPress(KeyCode.Space); // wait for this function to return
            playerAnimator.speed = speed;
        }


        private IEnumerator waitForKeyPress(KeyCode key)
        {
            bool done = false;
            while (!done) // essentially a "while true", but with a bool to break out naturally
            {
                if (Input.GetKeyDown(key))
                {
                    done = true; // breaks the loop
                }
                yield return null; // wait until next frame, then continue execution from here (loop continues)
            }

            // now this function returns
        }


        // Update is called once per frame
        void Update()
        {
            // print(
            //     playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length * 
            //    (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1) * playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);
            // int currentFrame = (int)(animationClip[0].weight * (animationClip[0].clip.length * animationClip[0].clip.frameRate));
            // float time = animationClip[0].clip.length * animationInfo.normalizedTime;
            // print("---------------------");
            // print("time: " + time);
            // print("animationClip[0]: " + animationClip[0]);
            // print("animationClip[0].clip: " + animationClip[0].clip);
            // print("animationClip[0].clip.length: " + animationClip[0].clip.length);
            // print("animationClip[0].clip.frameRate: " + animationClip[0].clip.frameRate);
            // print("animationClip[0].weight: " + animationClip[0].weight);
            // print("animationClip.Length: " + animationClip.Length);
            // print("currentFrame: " + currentFrame);
        }


        public void PrintEvent(string s)
        {
            Debug.Log("PrintEvent: " + s + " called at: " + Time.time);
        }
    }

}